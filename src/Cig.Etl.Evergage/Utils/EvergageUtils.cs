using System;
using System.Data;
using System.Data.SqlClient;
using Cig.Etl.Shared.Utils;

namespace Cig.Etl.Evergage.Utils
{
    public static class EvergageUtils
    { 
        public static void UpdateLastExecutionDate(string jobName, DateTime lastExecutionDate, SqlConnection sqlConnection, SqlTransaction sqlTransaction)
        {

            var updateQuery = @"INSERT INTO config.Evergage_DataExportLog
                                 (JobName, LastExecutionDate)
                                 VALUES(@JobName,@LastExecutionDate)";

            var jobNameParam = new SqlParameter("JobName", SqlDbType.NVarChar);
            var lastExecutionDateParam = new SqlParameter("LastExecutionDate", SqlDbType.DateTime2);

            jobNameParam.Value = jobName;
            lastExecutionDateParam.Value = lastExecutionDate;

            SqlServerUtils.ExecuteCommandReturnNone(updateQuery, sqlConnection, sqlTransaction, jobNameParam, lastExecutionDateParam);
        }
    }
}
