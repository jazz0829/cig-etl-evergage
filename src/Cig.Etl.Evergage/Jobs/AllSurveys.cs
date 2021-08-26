using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Cig.Etl.Evergage.Configuration;
using Cig.Etl.Evergage.Jobs.Contracts;
using Cig.Etl.Evergage.Model;
using Cig.Etl.Evergage.Utils;
using Cig.Etl.Shared.Utils;
using CsvHelper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cig.Etl.Evergage.Jobs
{
	public class AllSurveys : IJob
	{
		public string Name { get { return "AllSurveys"; } }
		readonly ILogger<ConsultancySurvey> _jobLogger;
		private readonly string _destinationConnectionString;
		private readonly string _destinationTable;
		private readonly string _sourceFolder;


		public AllSurveys(IOptions<List<Setting>> jobsOptions, ILogger<ConsultancySurvey> jobLogger)
		{
			this._jobLogger = jobLogger;
			var jobsOption = jobsOptions.Value.First(opt => opt.Name.Equals(this.Name, StringComparison.OrdinalIgnoreCase));
			this._destinationConnectionString = jobsOption.DestinationConnectionString;
			this._sourceFolder = jobsOption.SourceFolder;
			this._destinationTable = jobsOption.DestinationTable;
		}

		public async Task ExecuteAsync()
		{
			try
			{
				_jobLogger.LogInformation("Starting Job {JobName}", this.Name);

				if (Directory.Exists(this._sourceFolder))
				{
					var folders = new DirectoryInfo(this._sourceFolder).GetDirectories()
						.OrderByDescending(d => d.LastWriteTimeUtc).First();

					var filesList = Directory.GetFiles(folders.FullName, "*.csv", SearchOption.TopDirectoryOnly);

					foreach (var file in filesList)
					{
						_jobLogger.LogInformation("Getting data from file {file}", file);

						var table = GenerateDataTable(file);

						ImportDataTable(table);

					}
				}


				this._jobLogger.LogInformation("finished job: {JobName}", this.Name);
			}
			catch (SqlException ex)
			{
				this._jobLogger.LogError(ex, "SQLException in {JobName} ", this.Name);
				throw;
			}
			catch (Exception ex)
			{
				this._jobLogger.LogError(ex, "Exception in {JobName} ", this.Name);
				throw;
			}
		}

		private DataTable GenerateDataTable(string file)
		{

			var dataTable = new DataTable();
			var properties = typeof(AllSurvey).GetProperties().ToList();

			ConfigureDataTableColumns(dataTable, properties);

			using (var reader = File.OpenText(file))
			{
				var csv = new CsvReader(reader);
				csv.Configuration.HasHeaderRecord = true;
				while (csv.Read())
				{
					var row = dataTable.NewRow();
					var record = csv.GetRecord<AllSurvey>();
					foreach (var prop in properties)
					{
						var value = prop.GetValue(record, null);
						row[prop.Name] = value ?? (object)DBNull.Value;
					}

					dataTable.Rows.Add(row);
				}
			}

			return dataTable;
		}

		private static void ConfigureDataTableColumns(DataTable table, List<PropertyInfo> properties)
		{
			var columnTypeMapping = new Dictionary<string, Type>();

			foreach (var property in properties)
			{
				columnTypeMapping.Add(property.Name, property.PropertyType);
			}

			foreach (var column in columnTypeMapping.Keys)
			{
				var type = columnTypeMapping[column];

				var dataColumn = new DataColumn(column, type);
				if (type == typeof(DateTime))
				{
					dataColumn.DateTimeMode = DataSetDateTime.Utc;
				}

				table.Columns.Add(dataColumn);
			}
		}

		private void ImportDataTable(DataTable table)
		{
			using (var conn = SqlServerUtils.OpenConnection(this._destinationConnectionString))
			{
				using (var tran = conn.BeginTransaction())
				{
					_jobLogger.LogInformation($"Truncating the table {table} ...");
					SqlServerUtils.TruncateSqlTable(_destinationTable, _destinationConnectionString);

					_jobLogger.LogInformation($"Bulk copy data to table {_destinationConnectionString} ...");
					SqlServerUtils.BulkCopy(this._destinationConnectionString, this._destinationTable, table, conn, tran);

					_jobLogger.LogInformation($"Update last execution date and time...");
					EvergageUtils.UpdateLastExecutionDate(this.Name, DateTime.UtcNow, conn, tran);

					tran.Commit();
				}
			}
		}


	}
}