using Microsoft.Azure.Functions.Worker;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace AdventureWorksBulkCleanupFunctionApp
{
    public class DatabaseCleanup
    {
        private readonly ILogger _logger;

        public DatabaseCleanup(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<DatabaseCleanup>();
        }

        [Function("DatabaseCleanup")]
        public async Task Run([TimerTrigger("*/15 * * * * *", RunOnStartup = false)] MyInfo myTimer)
        {
            _logger.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
            _logger.LogInformation($"Next timer schedule at: {myTimer?.ScheduleStatus?.Next}");

            // Get the connection string from app settings and use it to create a connection.
            var str = Environment.GetEnvironmentVariable("sqldb_connection");
            using SqlConnection conn = new(str);
            conn.Open();
            
            var text = "UPDATE SalesLT.SalesOrderHeader " +
                       "SET [Status] = 5  WHERE ShipDate < GetDate();";

            using SqlCommand cmd = new(text, conn);

            // Execute the command and log the # rows affected.
            var rows = await cmd.ExecuteNonQueryAsync();
            _logger.LogInformation($"{rows} rows were updated");
        }
    }

    public class MyInfo
    {
        public MyScheduleStatus ScheduleStatus { get; set; }

        public bool IsPastDue { get; set; }
    }

    public class MyScheduleStatus
    {
        public DateTime Last { get; set; }

        public DateTime Next { get; set; }

        public DateTime LastUpdated { get; set; }
    }
}
