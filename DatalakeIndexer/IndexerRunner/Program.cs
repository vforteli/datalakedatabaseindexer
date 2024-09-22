using DatabaseIndexer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

var config = new ConfigurationBuilder().AddJsonFile($"appsettings.json", true, true).Build();
var loggerFactory = LoggerFactory.Create(o => o.AddSimpleConsole(c => c.SingleLine = true));
var logger = loggerFactory.CreateLogger<Program>();

var mockPaths = Utils.GetMockPaths();

var sqlConnectionFactory = new SqlConnectionFactory(config["connectionString"] ?? throw new ArgumentNullException("..."));
var datalakeIndexer = new DatalakeIndexer(sqlConnectionFactory, loggerFactory.CreateLogger<DatalakeIndexer>());


logger.LogInformation("Starting path upsert...");
var rowsAffected = datalakeIndexer.UpsertPathsAsync(mockPaths).ToBlockingEnumerable();
logger.LogInformation("Upsert done, rows affected {rows}", rowsAffected);
