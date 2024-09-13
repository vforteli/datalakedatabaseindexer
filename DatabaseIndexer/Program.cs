using DatabaseIndexer;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

var config = new ConfigurationBuilder().AddJsonFile($"appsettings.json", true, true).Build();
var loggerFactory = LoggerFactory.Create(o => o.AddSimpleConsole(c => c.SingleLine = true));
var logger = loggerFactory.CreateLogger<Program>();

var mockPaths = Utils.GetMockPaths();

var datalakeIndexer = new DatalakeIndexer(new SqlConnection(config["connectionString"]), loggerFactory.CreateLogger<DatalakeIndexer>());


logger.LogInformation("Starting path upsert...");
var rowsAffected = await datalakeIndexer.UpsertPathsAsync(mockPaths);
logger.LogInformation("Upsert done, rows affected {rows}", rowsAffected);
