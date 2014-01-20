using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TDDBrownBag
{
    class TDDBrownBag
    {
        static void Main(string[] args)
        {
            const string connectionString = "Server=epdbdev01;Database=Enrollment0133;Integrated Security=true";

            DateTime? lastProcessed = null;
            var today = DateTime.Today;
            var yesterday = DateTime.Today.AddDays(-100);
            var tomorrow = DateTime.Today.AddDays(1);

            // get last processed date

            using (var connection = new SqlConnection(connectionString))
            {
                var query = string.Format("SELECT ISNULL(MAX(ProcessDT), '{0}') FROM log_Process WHERE Process = 'ComcastDailyReport'", yesterday);

                using (var command = new SqlCommand(query, connection))
                {
                    connection.Open();

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            lastProcessed = reader[0] as DateTime?;
                        }
                    }
                }
            }
            
           // get report 

            using (var connection = new SqlConnection(connectionString))
            {
                var query = "SELECT TOP 5 ";
                query = query + "    am.AccountID * 17 as AccountID ";
                query = query + "    ,1000001 AS ComcastID ";
                query = query + "    ,am.ServiceStartDate AS [Start Date] ";
                query = query + "    ,am.ServiceEndDate AS [End Date] ";
                query = query + "    ,CASE WHEN StatusID IN (51,55) THEN 'Enroll-Request' ";
                query = query + "        WHEN StatusID IN (53,56) AND am.ServiceStartDate > '" + today + "' THEN 'Enroll-Accept' ";
                query = query + "        WHEN StatusID IN (57) THEN 'Enroll-Reject' ";
                query = query + "        WHEN StatusID IN (53,56) AND am.ServiceStartDate <= '" + today + "' AND ISNULL(am.ServiceEndDate, '" + tomorrow + "') > '" + today + "' THEN 'On Flow' ";
                query = query + "        WHEN StatusID IN (54,62) THEN 'Canceled' ";
                query = query + "    END Status ";
                query = query + "    ,CASE WHEN am.ServiceType = 1 THEN 'Electric' ELSE 'Gas' END AS Commodity ";
                query = query + "    ,Promotion ";
                query = query + "    ,am.Email ";
                query = query + "    ,'" + today + "' AS [Process Date] ";
                query = query + "FROM dbo.EPData_AccountMaster am ";
                query = query + "    LEFT JOIN dbo.Epdata_AccountMasterHistory H on h.accountID = am.accountID ";
                query = query + "    JOIN Accounts a ON a.AccountID = am.AccountID ";
                query = query + "    JOIN Promotions p on p.PromoCode = am.PromoCode ";
                query = query + "WHERE (am.InsertDT >= '" + lastProcessed + "' OR am.UpdateDT >= '" + lastProcessed + "') ";
                query = query + "    AND  ";
                query = query + "    ( ";
                query = query + "        (   h.HistoryInsertDT >= '" + lastProcessed  + "' AND (h.Status != am.Status or h.PromoCode != am.PromoCode) ) ";
                query = query + "    OR h.AccountID IS NULL ";
                query = query + "    )";

                using (var command = new SqlCommand(query, connection))
                {
                    connection.Open();

                    using (var reader = command.ExecuteReader())
                    {
                        Console.WriteLine("AccountID, ComcastID, StartDate, EndDate, Status, Commodity, Promotion, Email, ProcessedDate");
                        while (reader.Read())
                        {
                            Console.WriteLine(reader[0] + ", " + reader[1] + ", " + reader[2] + ", " + reader[3] + ", " + reader[4] + ", " + reader[5] + ", " + reader[6] + ", " + reader[7] + ", " + reader[8]);
                        }
                    }
                }
            }

            // add record to log process table

            using (var connection = new SqlConnection(connectionString))
            {
                using (var command = new SqlCommand())
                {
                    command.Connection = connection;
                    command.CommandText = "INSERT INTO dbo.log_Process (Process, ProcessDT) VALUES ('ComcastDailyReport', @val1)";
                    command.Parameters.AddWithValue("@val1", today);
                    try
                    {
                        connection.Open();
                        command.ExecuteNonQuery();
                    }
                    catch(SqlException e)
                    {
                        throw;
                    }
                }
            }
	
        }
    }
}
