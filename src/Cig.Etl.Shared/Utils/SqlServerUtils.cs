using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace Cig.Etl.Shared.Utils
{
    public static class SqlServerUtils
    {
        //https://msdn.microsoft.com/en-us/library/ms174396.aspx
        //6 = SQL Data Warehouse
        public static readonly int SqlDataWarehouseEngineEdition = 6;

        public static SqlConnection OpenConnection(string connectionString)
        {
            var sqlConnection = new SqlConnection(connectionString);
            sqlConnection.Open();
            return sqlConnection;
        }

        public static void ExecuteCommandReturnNone(string query, string connectionString, bool clearPool = false)
        {
            using (var sqlConnection = Retry.Do(() => OpenConnection(connectionString), TimeSpan.FromSeconds(60), 5))
            {
                using (var sqlTransaction = sqlConnection.BeginTransaction())
                {
                    ExecuteCommandReturnNone(query, sqlConnection, sqlTransaction);
                    sqlTransaction.Commit();
                    if (clearPool)
                    {
                        SqlConnection.ClearPool(sqlConnection);
                    }
                }
            }
        }


        public static void ExecuteCommandReturnNone(string query, SqlConnection sqlConnection,
            SqlTransaction sqlTransaction)
        {
            ExecuteCommandReturnNone(query, sqlConnection, sqlTransaction, null);
        }

        public static void ExecuteCommandReturnNone(string parametrizedCommand, SqlConnection sqlConnection,
            SqlTransaction sqlTransaction, params SqlParameter[] parameters)
        {
            using (var sqlCommand = sqlConnection.CreateCommand())
            {
                if (parameters != null)
                {
                    foreach (var param in parameters)
                    {
                        sqlCommand.Parameters.Add(param);
                    }
                }

                sqlCommand.Transaction = sqlTransaction;
                sqlCommand.CommandText = parametrizedCommand;
                sqlCommand.CommandTimeout = 1200;
                sqlCommand.ExecuteNonQuery();
            }
        }

        public static int ExecuteCommandReturnInt(string query, string connectionString)
        {
            int numberRecords;
            using (var sqlConnection = Retry.Do(() => OpenConnection(connectionString), TimeSpan.FromSeconds(60), 5))
            {
                using (var sqlCommand = sqlConnection.CreateCommand())
                {
                    sqlCommand.CommandText = query;
                    sqlCommand.CommandTimeout = 1200;
                    numberRecords = (int) sqlCommand.ExecuteScalar();
                }
            }

            return numberRecords;
        }

        public static T ExecuteCommandReturnSingle<T>(string query, string connectionString)
        {
            T result;
            using (var sqlConnection = Retry.Do(() => OpenConnection(connectionString), TimeSpan.FromSeconds(60), 5))
            {
                using (var sqlCommand = sqlConnection.CreateCommand())
                {
                    sqlCommand.CommandText = query;
                    sqlCommand.CommandTimeout = 1200;
                    result = (T) sqlCommand.ExecuteScalar();
                }
            }

            return result;
        }

        public static T ExecuteCommandReturnSingle<T>(string query, SqlConnection sqlConnection,
            SqlTransaction sqlTransaction)
        {
            T result;

            using (var sqlCommand = sqlConnection.CreateCommand())
            {
                sqlCommand.Transaction = sqlTransaction;
                sqlCommand.CommandText = query;
                sqlCommand.CommandTimeout = 1200;
                result = (T) sqlCommand.ExecuteScalar();
            }

            return result;
        }

        public static SqlDataReader ExecuteCommandReturnReader(string query, string connectionString)
        {
            SqlDataReader reader;
            var sqlConnection = Retry.Do(() => OpenConnection(connectionString), TimeSpan.FromSeconds(60), 5);
            using (var sqlCommand = sqlConnection.CreateCommand())
            {
                sqlCommand.CommandText = query;
                sqlCommand.CommandTimeout = 1200;
                reader = sqlCommand.ExecuteReader(CommandBehavior.CloseConnection);
            }

            return reader;
        }

        public static int GetSqlServerEngineEdition(string connectionString)
        {
            var editionQuery = "select SERVERPROPERTY('EngineEdition')";
            var engineEdition = ExecuteCommandReturnInt(editionQuery, connectionString);
            return engineEdition;
        }

        public static void TruncateSqlTable(string sqlTable, string connectionString)
        {
            ExecuteCommandReturnNone($"TRUNCATE TABLE {sqlTable}", connectionString);
        }

        public static void DropSqlDatabase(string connectionString)
        {
            var builder = new SqlConnectionStringBuilder(connectionString);
            var dropQuery = new StringBuilder();
            dropQuery.Append($"IF EXISTS(select * from sys.databases where name = '{builder.InitialCatalog}') ");
            dropQuery.Append($"ALTER DATABASE {builder.InitialCatalog} SET SINGLE_USER WITH ROLLBACK IMMEDIATE ");
            dropQuery.Append(
                $"USE master IF EXISTS(select * from sys.databases where name = '{builder.InitialCatalog}') DROP DATABASE {builder.InitialCatalog}");
            ExecuteQuery(dropQuery.ToString(), connectionString);
        }

        public static void RestoreSqlDatabase(string databaseName, string backupFileFullPath, string connectionString,
            string storagePath)
        {
            var logicalNames = GetDbLogicalNames(backupFileFullPath, connectionString);
            var restoreQuery = new StringBuilder();
            restoreQuery.Append(
                $@"RESTORE DATABASE[{databaseName}] FROM  DISK = N'{backupFileFullPath}' WITH FILE = 1, ");
            restoreQuery.Append($@" MOVE N'{logicalNames["LogicalNameData"]}' TO N'{storagePath}{databaseName}.mdf', ");
            restoreQuery.Append(
                $@" MOVE N'{logicalNames["LogicalNameLog"]}' TO N'{storagePath}{databaseName}_log.ldf', REPLACE, NOUNLOAD, STATS = 5 ");
            ExecuteQuery(restoreQuery.ToString(), connectionString);
        }

        private static void ExecuteQuery(string sqlQuery, string connectionString)
        {
            using (var sqlConnection = Retry.Do(() => OpenConnection(connectionString), TimeSpan.FromSeconds(60), 5))
            {
                using (var sqlCommand = sqlConnection.CreateCommand())
                {
                    sqlCommand.CommandText = sqlQuery;
                    sqlCommand.CommandTimeout = 1200;
                    sqlCommand.ExecuteNonQuery();
                }
            }
        }

        public static string ChangeInitialCatalogConnectionString(string databaseName, string sqlConnectionString)
        {
            var builder = new SqlConnectionStringBuilder(sqlConnectionString)
            {
                InitialCatalog = databaseName
            };
            return builder.ConnectionString;
        }


        public static Dictionary<string, string> GetDbLogicalNames(string dbPath, string connectionString)
        {
            var logicalNames = new Dictionary<string, string>();

            var sb = new StringBuilder();
            sb.Append($"RESTORE FILELISTONLY FROM DISK ='{dbPath}'");
            var query = sb.ToString();
            using (var reader = SqlServerUtils.ExecuteCommandReturnReader(query, connectionString))
            {
                while (reader.Read())
                {
                    if ((string) reader.GetValue(2) == "D")
                    {
                        logicalNames.Add("LogicalNameData", (string) reader.GetValue(0));
                    }

                    if ((string) reader.GetValue(2) == "L")
                    {
                        logicalNames.Add("LogicalNameLog", (string) reader.GetValue(0));
                    }
                }
            }

            return logicalNames;
        }

        public static void ConvertTypesNotSupportedByAzureDw(DataTable table)
        {
            ConvertSqlColumnType(table, "Geography", "string");
        }

        public static void ConvertSqlColumnType(DataTable table, string originalType, string newType)
        {
            var newColumns = new Dictionary<string, DataColumn>();

            var numberOfColumns = table.Columns.Count;

            for (var i = 0; i < numberOfColumns; i++)
            {
                var column = table.Columns[i];
                if (column.DataType.ToString().Contains(originalType))
                {
                    var tempColumnName = $"temporary_{column.ColumnName}";
                    var newColumn = new DataColumn(tempColumnName, newType.GetType());
                    table.Columns.Add(newColumn);
                    newColumns.Add(column.ColumnName, newColumn);
                }
            }

            foreach (DataRow row in table.Rows)
            {
                foreach (var columnKey in newColumns.Keys)
                {
                    row[$"temporary_{columnKey}"] = row[columnKey].ToString();
                }
            }

            foreach (var columnKey in newColumns.Keys)
            {
                table.Columns.Remove(columnKey);
                newColumns[columnKey].ColumnName = columnKey;
            }
        }

        public static IEnumerable<string> GetColumnNames(string conStr,string schema, string tableName)
        {
            var result = new List<string>();
            var builder = new SqlCommandBuilder();
            string escapedTableName = builder.QuoteIdentifier(tableName);
            using (var sqlCon = new SqlConnection(conStr))
            {
                sqlCon.Open();
                var sqlCmd = sqlCon.CreateCommand();
                sqlCmd.CommandText = $"select * from {schema}.{escapedTableName} where 1=0"; // No data wanted, only schema
                sqlCmd.CommandType = CommandType.Text;

                var sqlDR = sqlCmd.ExecuteReader();
                var dataTable = sqlDR.GetSchemaTable();

                foreach (DataRow row in dataTable.Rows) result.Add(row.Field<string>("ColumnName"));
            }

            return result;
        }

        public static void AddNVarCharColumnsToTable(string schema, string tableName, IEnumerable<string> columns, string connectionString)
        {
            var builder = new SqlCommandBuilder();
            string escapedTableName = builder.QuoteIdentifier(tableName);
            int maxLength;
            var exceptions = new List<string>
            {
                "fullcancellationreason_nonaccountancy1a1c1",
                "fullcancellationreason_nonaccountancy1d1",
                "fullcancellationreason_nonaccountancy1a1a1"
            };
            foreach (var column in columns)
            {
                //This is the only column that contains a text longer than 380 characheters, if we force all columns to be nvarchar(max) or even nvarchar(4000) we'll have a sql exception
                //Cannot create a row of size 8937 which is greater than the allowable maximum of 8060
                maxLength = exceptions.Contains(column.ToLower()) ? 500 : 160;
                var query = $"ALTER TABLE {schema}.{escapedTableName}  ADD {builder.QuoteIdentifier(column)} NVARCHAR({maxLength});";
                ExecuteCommandReturnNone(query, connectionString);
            }
        }

        public static void  BulkCopy(string connectionString, string destinationTableName, DataTable source, SqlConnection connection, SqlTransaction transaction)
        {
            using (SqlBulkCopy bulkCopy = new SqlBulkCopy(connection, SqlBulkCopyOptions.Default, transaction))
            {
                bulkCopy.DestinationTableName = destinationTableName;
                bulkCopy.WriteToServer(source);
            }
        }
    }
}