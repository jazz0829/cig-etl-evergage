using Cig.Etl.Evergage.Configuration;
using Cig.Etl.Evergage.Jobs.Contracts;
using Cig.Etl.Evergage.Utils;
using Cig.Etl.Shared.Utils;
using Cig.Evergage.Client.Contracts;
using CsvHelper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Cig.Etl.Evergage.Jobs
{
	public class ConsultancySurvey : IJob
	{
		private readonly string destinationConnectionString;
		private readonly string destinationTable;
		IEvergageClient evergageClient;
		ILogger<ConsultancySurvey> jobLogger;

		public ConsultancySurvey(IOptions<List<Setting>> jobsOptions, IEvergageClient evergageClient, ILogger<ConsultancySurvey> jobLogger)
		{
			var jobsOption = jobsOptions.Value.First(opt => opt.Name.Equals(this.Name, StringComparison.OrdinalIgnoreCase));
			this.destinationConnectionString = jobsOption.DestinationConnectionString;
			this.destinationTable = jobsOption.DestinationTable;
			this.evergageClient = evergageClient;
			this.jobLogger = jobLogger;
		}

		public string Name { get { return "ConsultancySurvey"; } }

		public async Task ExecuteAsync()
		{
			jobLogger.LogInformation("Starting Job {JobName}", this.Name);

			try
			{
				var records = await GetSurveyData();
				if (records != null)
				{
					StoreSurveyData(records);
				}
				else
				{
					jobLogger.LogInformation("{JobName}: No survey results found", this.Name);
				}

			}
			catch (HttpRequestException httpEx)
			{
				jobLogger.LogError(httpEx, "HttpException in {JobName} ", this.Name);
				throw;
			}
			catch (SqlException sqlEx)
			{

				jobLogger.LogError(sqlEx, "SQLException in {JobName} ", this.Name);
				throw;
			}
			catch (Exception ex)
			{
				jobLogger.LogError(ex, "Unexpected Exception in {JobName} ", this.Name);
				throw;
			}
			jobLogger.LogInformation("Ending Job {JobName}", this.Name);
		}

		private async Task<DataTable> GetSurveyData()
		{
			var csvData = await evergageClient.GetSurveyData();

			using (TextReader sr = new StringReader(csvData))
			using (var csvReader = new CsvReader(sr))
			{
				csvReader.Configuration.HasHeaderRecord = true;
				csvReader.Configuration.Delimiter = ",";

				DataTable table = CreateDataTable();

				foreach (var record in csvReader.GetRecords<Model.ConsultancySurvey>())
				{
					var dr = table.NewRow();

					dr["UserId"] = record.UserId;
					dr["TimeStamp"] = record.TimeStamp;
					dr["Solution"] = record.Solution;
					dr["RequestId"] = record.RequestId;
				    dr["ContactId"] = record.ContactId;
                    dr["Question4"] = record.Question4;
					dr["Question5"] = record.Question5;
					dr["Question6"] = record.Question6;
					dr["Question7"] = record.Question7;
					dr["Question8"] = record.Question8;
					dr["Question1"] = record.Question1;
					dr["Question2"] = record.Question2;
				    dr["Question9"] = record.Question9;
				    dr["Question9_verbatim"] = record.Question9Verbatim;

                    table.Rows.Add(dr);
				}
				return table;
			}
		}

		private DataTable CreateDataTable()
		{
			var dt = new DataTable(destinationTable);

			dt.Columns.Add("UserId");
			dt.Columns.Add("TimeStamp", typeof(DateTime));
			dt.Columns.Add("Solution");
			dt.Columns.Add("RequestId");
		    dt.Columns.Add("ContactId");
            dt.Columns.Add("Question4");
			dt.Columns.Add("Question5");
			dt.Columns.Add("Question6");
			dt.Columns.Add("Question7");
			dt.Columns.Add("Question8");
			dt.Columns.Add("Question1");
			dt.Columns.Add("Question2");
		    dt.Columns.Add("Question9");
		    dt.Columns.Add("Question9_verbatim");
            dt.Columns.Add("cigcopytime", typeof(DateTime)).DefaultValue = DateTime.UtcNow;
			return dt;
		}


		private void StoreSurveyData(DataTable dataTable)
		{
			using (var conn = Retry.Do(() => SqlServerUtils.OpenConnection(this.destinationConnectionString), TimeSpan.FromSeconds(60), 5))
			{
				using (var tran = conn.BeginTransaction())
				{
					
					jobLogger.LogInformation($"Truncating the table {destinationTable} ...");
					SqlServerUtils.TruncateSqlTable(destinationTable, destinationConnectionString);

					jobLogger.LogInformation($"Bulk copy data to table {destinationTable} ...");
					SqlServerUtils.BulkCopy(destinationConnectionString, destinationTable, dataTable, conn, tran);

					jobLogger.LogInformation($"Update last execution date and time...");
					EvergageUtils.UpdateLastExecutionDate(this.Name, DateTime.UtcNow, conn, tran);

					tran.Commit();
				}
			}
		}
	}
}
