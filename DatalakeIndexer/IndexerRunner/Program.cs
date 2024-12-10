using DatabaseIndexer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;
using PGDatabaseIndexer;

var config = new ConfigurationBuilder().AddJsonFile($"appsettings.json", true, true).Build();
var loggerFactory = LoggerFactory.Create(o => o.AddSimpleConsole(c => c.SingleLine = true));
var logger = loggerFactory.CreateLogger<Program>();

var mockPaths = Utils.GetMockPaths(10, 1000, 1000);

// logger.LogInformation("Starting path upsert...");
// var cassandraIndexer = new CassandraIndexer(loggerFactory.CreateLogger<CassandraIndexer>());
// await cassandraIndexer.UpsertPathsAsync(mockPaths, 100);
// logger.LogInformation("Upsert done, rows affected {rows}", -1);

var dataSource =
    new NpgsqlDataSourceBuilder(config["pgConnectionString"] ?? throw new ArgumentNullException("pgConnectionString"))
        .Build();

var pgServerIndexer = new PGServerIndexer(dataSource, loggerFactory.CreateLogger<PGServerIndexer>());

logger.LogInformation("Starting path upsert...");
var rowsAffected = pgServerIndexer.UpsertPathsAsync(mockPaths.Select(o => new PGDatabaseIndexer.PathRowType
{
    created_on = o.CreatedOn,
    deleted_on = o.DeletedOn,
    etag = o.ETag,
    filesystem_name = o.FilesystemName,
    last_modified = o.LastModified,
    path = o.Path,
})).ToBlockingEnumerable();
logger.LogInformation("Upsert done, rows affected {rows}", rowsAffected.Count());

//
//
// var sqlConnectionFactory =
//     new SqlConnectionFactory(config["connectionString"] ?? throw new ArgumentNullException("..."));
// var datalakeIndexer = new SqlServerIndexer(sqlConnectionFactory, loggerFactory.CreateLogger<SqlServerIndexer>());
//
//
// logger.LogInformation("Starting path upsert...");
// var rowsAffected = datalakeIndexer.UpsertPathsAsync(mockPaths).ToBlockingEnumerable();
// logger.LogInformation("Upsert done, rows affected {rows}", rowsAffected.Count());