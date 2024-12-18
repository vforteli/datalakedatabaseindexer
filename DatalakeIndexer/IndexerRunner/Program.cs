using System.Text.Json;
using DatabaseIndexer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;
using PGDatabaseIndexer;
using PathMetadataRowType = PGDatabaseIndexer.PathMetadataRowType;

var config = new ConfigurationBuilder().AddJsonFile($"appsettings.json", true, true).Build();
var loggerFactory = LoggerFactory.Create(o => o.AddSimpleConsole(c => c.SingleLine = true));
var logger = loggerFactory.CreateLogger<Program>();

var mockPaths = Utils.GetMockPaths(10, 1000, 1000);

// logger.LogInformation("Starting path upsert...");
// var cassandraIndexer = new CassandraIndexer(loggerFactory.CreateLogger<CassandraIndexer>());
// await cassandraIndexer.UpsertPathsAsync(mockPaths, 100);
// logger.LogInformation("Upsert done, rows affected {rows}", -1);

using var dataSource =
    new NpgsqlDataSourceBuilder(config["pgConnectionString"] ?? throw new ArgumentNullException("pgConnectionString"))
        .Build();

var pgServerIndexer = new PGServerIndexer(dataSource, loggerFactory.CreateLogger<PGServerIndexer>());

logger.LogInformation("Starting path upsert...");

var mockMetadata = new
{
    someproperty = "somevalue",
    anotherproperty = "anothervalue",
    hurr = "durr",
    foooooo = "some slightly longer property goes here",
};

var mockMetadataJson = JsonSerializer.Serialize(mockMetadata);

foreach (var mockPathChunk in mockPaths.Chunk(100000))
{
    var upsertedPaths = pgServerIndexer.UpsertPathsAsync(mockPathChunk.Select(o => new PGDatabaseIndexer.PathRowType
    {
        created_on = o.CreatedOn,
        deleted_on = o.DeletedOn,
        etag = o.ETag,
        filesystem_name = o.FilesystemName,
        last_modified = o.LastModified,
        path = o.Path,
    })).ToBlockingEnumerable();

    await pgServerIndexer.UpsertPathsMetadataAsync(upsertedPaths.Select(o =>
        new PathMetadataRowType
        {
            etag = "notrelevant",
            path_key = o.path_key,
            metadata_json = mockMetadataJson,
        }));
}

logger.LogInformation("Upsert done");

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