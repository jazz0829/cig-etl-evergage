using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Amazon;
using Amazon.Kinesis;
using Cig.Etl.Evergage.Configuration;
using Cig.Etl.Evergage.Configuration.Contracts;
using Cig.Etl.Evergage.Jobs.Contracts;
using Cig.Etl.Evergage.Logging;
using Cig.Etl.Evergage.Utils;
using Cig.Etl.Shared.Utils;
using Cig.Evergage.Client;
using Cig.Evergage.Client.Contracts;
using Eol.Cig.Etl.Kinesis.Producer;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cig.Etl.Evergage.Jobs
{
    public abstract class Job<T> : IJob
    {
        private readonly IEvergageClient evergageClient;
        private readonly ILogger<T> logger;
        private readonly Log4NetNLogger<KinesisWriter> log4NetLogger;
        private readonly string segmentId;
        private readonly string destinationConnectionString;
        private readonly string destinationTable;
        private readonly int batchSize;
        public string Name { get; } = typeof(T).Name;
        public string TableName => this.destinationTable.Substring(this.destinationTable.IndexOf(".", StringComparison.OrdinalIgnoreCase) + 1);
        public string Schema => this.destinationTable.Substring(0, this.destinationTable.IndexOf(".", StringComparison.Ordinal));
        private readonly IAmazonKinesis kinesisClient;
        private readonly bool isStreamingToKinesisEnabled;
        private readonly string s3Prefix;
        private readonly string kinesisStreamName;

        protected Job(IOptions<List<Setting>> jobsOptions, IAwsConfiguration awsConfiguration, IEvergageClient evergageClient, ILogger<T> logger, ILogger<KinesisWriter> kinesisLogger)
        {
            var jobsOption = jobsOptions.Value.First(opt => opt.Name.Equals(this.Name, StringComparison.OrdinalIgnoreCase));
            this.segmentId = jobsOption.SegmentId;
            this.destinationConnectionString = jobsOption.DestinationConnectionString;
            this.destinationTable = jobsOption.DestinationTable;
            this.batchSize = jobsOption.BatchSize == 0 ? 100 : jobsOption.BatchSize;
            this.evergageClient = evergageClient;
            this.logger = logger;
            this.kinesisClient = new AmazonKinesisClient(awsConfiguration.AwsAccessKeyId, awsConfiguration.AwsSecretAccessKey, RegionEndpoint.EUWest1);
            this.isStreamingToKinesisEnabled = awsConfiguration.IsStreamingEnabled;
            this.s3Prefix = awsConfiguration.S3Prefix;
            this.kinesisStreamName = awsConfiguration.AwsKinesisStreamName;
            log4NetLogger = new Log4NetNLogger<KinesisWriter>(kinesisLogger);
        }

        public async Task ExecuteAsync()
        {
            try
            {
                this.logger.LogInformation("starting job: {JobName}", this.Name);
                var executionDateTime = DateTime.UtcNow;
                Func<string, int, int, Task<IEnumerable<SegmentResponse>>> httpFunc = async (segId, pageNr, pageSize) =>
                    await this.evergageClient.Get<SegmentResponse>(segId, pageNr, pageSize);
                EvergageClientBatcher batcher = new EvergageClientBatcher(this.segmentId, this.batchSize);
                var segmentResponses = (await batcher.MakeBatchRequestAndAggregate(httpFunc, SanitizeSegmentResponses)).ToList();
                if (segmentResponses.Any())
                {
                    var columns = SqlServerUtils.GetColumnNames(this.destinationConnectionString, this.Schema, this.TableName);

                    var dropColumnsSql = new StringBuilder();
                    foreach (var column in columns.Except(new List<string> { "Name", "AccountName", "SegmentJoined" }))
                    {
                        dropColumnsSql.AppendLine($"ALTER TABLE {this.destinationTable} DROP COLUMN [{column}]; ");
                    }

                    if (dropColumnsSql.Length > 0)
                    {
                        this.logger.LogInformation("Running the sql query to drop old columns...");

                        this.logger.LogInformation(dropColumnsSql.ToString());

                        SqlServerUtils.ExecuteCommandReturnNone(dropColumnsSql.ToString(), this.destinationConnectionString);
                    }

                    var segments = segmentResponses.SelectMany(r => r.SegmentInputValues.Keys).Distinct().ToList();

                    this.logger.LogInformation($"Adding new columns [{string.Join(",", segments)}] to the table [{this.TableName}] ...");

                    SqlServerUtils.AddNVarCharColumnsToTable(this.Schema, this.TableName, segments, this.destinationConnectionString);

                    this.logger.LogInformation($"Truncating the table {this.destinationTable} ...");
                    SqlServerUtils.TruncateSqlTable(this.destinationTable, this.destinationConnectionString);

                    var dataTable = CreateDataTableEvergageJob(segmentResponses, segments);

                    using (var conn = Retry.Do(() => SqlServerUtils.OpenConnection(this.destinationConnectionString), TimeSpan.FromSeconds(60), 5))
                    {
                        using (var tran = conn.BeginTransaction())
                        {
                            SqlServerUtils.BulkCopy(this.destinationConnectionString, this.destinationTable, dataTable, conn, tran);
                            EvergageUtils.UpdateLastExecutionDate(this.Name, executionDateTime, conn, tran);
                            tran.Commit();
                        }
                    }

                    PostProcess(dataTable);
                }
                this.logger.LogInformation("finished job: {JobName}", this.Name);
            }
            catch (HttpRequestException httpEx)
            {
                this.logger.LogError(httpEx, "HttpException in {JobName} ", this.Name);
                throw;
            }
            catch (SqlException sqlEx)
            {
                this.logger.LogError(sqlEx, "SQLException in {JobName} ", this.Name);
                throw;
            }

        }

        private void PostProcess(DataTable dataTable)
        {
            if (this.isStreamingToKinesisEnabled)
            {
                this.logger.LogInformation("Start ingesting data to AWS S3 ...");
                var kinesisProducer = new KinesisWriter(
                    this.log4NetLogger,
                    this.kinesisClient,
                    kinesisStreamName,
                    this.s3Prefix,
                    this.Name);

                kinesisProducer.IngestData(dataTable);
            }
        }

        public long GetDateTimeEpochMillis(DateTime dateTime)
        {
            return new DateTimeOffset(dateTime, TimeSpan.Zero).ToUnixTimeMilliseconds();
        }

        private IEnumerable<SegmentResponse> SanitizeSegmentResponses(IEnumerable<SegmentResponse> segmentResponses)
        {
            return segmentResponses.Select(SanitizeSegmentResponse);
        }

        private SegmentResponse SanitizeSegmentResponse(SegmentResponse segmentResponse)
        {
            var sanitizedKeyValuePairs = segmentResponse.SegmentInputValues.Select(kvp =>
               new KeyValuePair<string, string>(SanitizeSegmentValue(kvp.Key), kvp.Value));
            segmentResponse.SegmentInputValues = sanitizedKeyValuePairs.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            return segmentResponse;
        }

        private string SanitizeSegmentValue(string segmentValue)
        {
            return segmentValue.Substring(0, Math.Min(128, segmentValue.Length))
                .Replace("\"", String.Empty)
                .Replace(" ", "_");
        }


        private DataTable CreateDataTableEvergageJob(IEnumerable<SegmentResponse> segmentResponses, IEnumerable<string> segmentColumns)
        {
            var table = new DataTable(this.Name);
            table.Columns.Add("Name", typeof(string));
            table.Columns.Add("AccountName", typeof(string));
            table.Columns.Add("SegmentJoined", typeof(DateTime));
            var columnList = segmentColumns.ToList();
            foreach (var column in columnList)
            {
                table.Columns.Add(column, typeof(string));
            }

            foreach (var segmentResponse in segmentResponses)
            {
                var dr = table.NewRow();
                dr["Name"] = segmentResponse.Name;
                dr["AccountName"] = segmentResponse.AccountName;
                dr["SegmentJoined"] = segmentResponse.SegmentJoined;

                foreach (var column in columnList)
                {
                    dr[column] = segmentResponse.SegmentInputValues[column];
                }
                table.Rows.Add(dr);
            }

            table.AcceptChanges();
            return table;
        }
    }
}
