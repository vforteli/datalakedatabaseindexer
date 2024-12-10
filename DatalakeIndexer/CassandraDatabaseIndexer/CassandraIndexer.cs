using System.Diagnostics;
using System.Net;
using Cassandra;
using Microsoft.Extensions.Logging;

namespace DatabaseIndexer;

public class CassandraIndexer(ILogger<CassandraIndexer> logger)
{
    public async Task UpsertPathsAsync(IEnumerable<PathRowType> paths, int chunkSize = 50000)
    {
        var cluster = Cluster
            .Builder()
            .AddContactPoints(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 9042))
            .Build();

        var session = await cluster.ConnectAsync("indexer");

        var statement = session.Prepare("""
            INSERT INTO paths (filesystem_name, path, created_on, last_modified, path_key, etag, path_segments) 
            VALUES (?, ?, ?, ?, ?, ?, ?)
            """);

        var totalRowsAffected = 0;
        var stopwatch = Stopwatch.StartNew();


        foreach (var chunk in paths.Chunk(chunkSize))
        {
            var batch = new BatchStatement();

            foreach (var row in chunk)
            {
                batch.Add(statement.Bind(row.FilesystemName, row.Path, row.CreatedOn, row.LastModified, row.PathKey, row.ETag, SplitPrefixes(row.Path)));
            }

            await session.ExecuteAsync(batch);

            totalRowsAffected += chunk.Length;
            var seconds = stopwatch.Elapsed.TotalSeconds;

            logger.LogInformation(
               "Total rows affected: {rows} after {elapsed}, rps: {rps}",
               totalRowsAffected,
               seconds,
               seconds > 0 ? (totalRowsAffected / seconds) : 0);
        }
    }


    public List<string> SplitPrefixes(string input)
    {
        var segments = input.Split("/");

        return Enumerable.Range(1, segments.Length).Select(o =>
        {
            return string.Join('/', segments[..o]);
        }).ToList();
    }

    public Task<int> UpsertPathsMetadataAsync(IEnumerable<PathMetadataRowType> paths, int chunkSize = 50000)
    {
        throw new NotImplementedException();
    }
}
