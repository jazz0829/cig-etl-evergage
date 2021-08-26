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
	public class RelationshipSurvey : IJob
	{
		private readonly string destinationConnectionString;
		private readonly string destinationTable;
		private readonly IEvergageClient evergageClient;
		private readonly ILogger<RelationshipSurvey> jobLogger;
	    private readonly string dutchSurveyId;
	    private readonly string englishSurveyId;

		public RelationshipSurvey(IOptions<List<Setting>> jobsOptions, IEvergageClient evergageClient, ILogger<RelationshipSurvey> jobLogger)
		{
			var jobsOption = jobsOptions.Value.First(opt => opt.Name.Equals(this.Name, StringComparison.OrdinalIgnoreCase));
			this.destinationConnectionString = jobsOption.DestinationConnectionString;
			this.destinationTable = jobsOption.DestinationTable;
		    this.dutchSurveyId = jobsOption.DutchSurveyId;
		    this.englishSurveyId = jobsOption.EnglishSurveyId;
			this.evergageClient = evergageClient;
			this.jobLogger = jobLogger;
		}

		public string Name { get { return "RelationshipSurvey"; } }

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
			var nlCsvData = await evergageClient.GetRelationshipSurveyData(dutchSurveyId);
		    var enCsvData = await evergageClient.GetRelationshipSurveyData(englishSurveyId);

		    DataTable nlTable = GetSurveyDataAsDataTable(nlCsvData);
            DataTable enTable = GetSurveyDataAsDataTable(enCsvData);
		    nlTable.Merge(enTable);

            return nlTable;

		}

	    private DataTable GetSurveyDataAsDataTable(string csvData)
	    {
	        DataTable table = CreateDataTable();
            using (TextReader sr = new StringReader(csvData))
            using (var csvReader = new CsvReader(sr))
            {
                csvReader.Configuration.HasHeaderRecord = true;
                csvReader.Configuration.Delimiter = ",";

                foreach (var record in csvReader.GetRecords<Model.RelationshipSurvey>())
                {
                    var dr = table.NewRow();
                    dr["UserId"] = record.UserId;
                    dr["TimeStamp"] = record.TimeStamp;
                    dr["UserId"] = record.UserId;
                    dr["SalesforceContactId"] = record.SalesforceContactId;
                    dr["Solution"] = System.Web.HttpUtility.UrlDecode(record.Solution);
                    dr["Question35"] = record.Question35;
                    dr["Question36"] = record.Question36;
                    dr["Question37"] = record.Question37;
                    dr["Question1"] = record.Question1;
                    dr["Question2"] = record.Question2;
                    dr["Question3"] = record.Question3;
                    dr["Question4"] = record.Question4;
                    dr["Question5"] = record.Question5;
                    dr["Question6"] = record.Question6;
                    dr["Question7"] = record.Question7;
                    dr["Question8"] = record.Question8;
                    dr["Question9"] = record.Question9;
                    dr["Question10"] = record.Question10;
                    dr["Question11"] = record.Question11;
                    dr["Question12"] = record.Question12;
                    dr["Question13"] = record.Question13;
                    dr["Question14"] = record.Question14;
                    dr["Question15"] = record.Question15;
                    dr["Question16"] = record.Question16;
                    dr["Question17"] = record.Question17;
                    dr["Question18"] = record.Question18;
                    dr["Question19"] = record.Question19;
                    dr["Question20"] = record.Question20;
                    dr["Question21"] = record.Question21;
                    dr["Question22"] = record.Question22;
                    dr["Question23"] = record.Question23;
                    dr["Question24"] = record.Question24;
                    dr["Question25"] = record.Question25;
                    dr["Question26"] = record.Question26;
                    dr["CloneOfQuestion27"] = record.CloneOfQuestion27;
                    dr["Question27"] = record.Question27;
                    dr["Question28"] = record.Question28;
                    dr["Question29"] = record.Question29;
                    dr["CloneOfQuestion30"] = record.CloneOfQuestion30;
                    dr["Question30"] = record.Question30;
                    dr["Question31"] = record.Question31;
                    dr["Question32"] = record.Question32;
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
			dt.Columns.Add("SalesforceContactId");
			dt.Columns.Add("Solution");
			dt.Columns.Add("Question35");
			dt.Columns.Add("Question36");
			dt.Columns.Add("Question37");
			dt.Columns.Add("Question1");
			dt.Columns.Add("Question2");
			dt.Columns.Add("Question3");
			dt.Columns.Add("Question4");
			dt.Columns.Add("Question5");
			dt.Columns.Add("Question6");
			dt.Columns.Add("Question7");
			dt.Columns.Add("Question8");
			dt.Columns.Add("Question9");
			dt.Columns.Add("Question10");
			dt.Columns.Add("Question11");
			dt.Columns.Add("Question12");
			dt.Columns.Add("Question13");
			dt.Columns.Add("Question14");
			dt.Columns.Add("Question15");
			dt.Columns.Add("Question16");
			dt.Columns.Add("Question17");
			dt.Columns.Add("Question18");
			dt.Columns.Add("Question19");
			dt.Columns.Add("Question20");
			dt.Columns.Add("Question21");
			dt.Columns.Add("Question22");
			dt.Columns.Add("Question23");
			dt.Columns.Add("Question24");
			dt.Columns.Add("Question25");
			dt.Columns.Add("Question26");
			dt.Columns.Add("CloneOfQuestion27");
			dt.Columns.Add("Question27");
			dt.Columns.Add("Question28");
			dt.Columns.Add("Question29");
			dt.Columns.Add("CloneOfQuestion30");
			dt.Columns.Add("Question30");
			dt.Columns.Add("Question31");
			dt.Columns.Add("Question32");
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
