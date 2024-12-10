using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.Json;
using Azure.Storage.Files.DataLake;
using DatabaseIndexer;
using DataLakeFileSystemClientExtension;
using Microsoft.Extensions.Logging;

namespace IndexerWorker;

/// <summary>
/// Indexer thingy
/// List paths in datalake and upsert paths into index. Updates metadata if files have been modified
/// </summary>
public class Indexer(ILogger<Indexer> logger, SqlServerIndexer datalakeIndexer, DataLakeServiceClient dataLakeServiceClient)
{
    /// <summary>
    /// Rescan filesystem path and upsert files and metadata
    /// </summary>
    public async Task ListPathsAsync(string filesystemName)
    {
        var fileSystemClient = dataLakeServiceClient.GetFileSystemClient(filesystemName);

        var paths = fileSystemClient.ListPathsParallelAsync("") // todo eh...
            .Where(o => !(o.IsDirectory ?? false))
            .Select(o => new PathRowType
            {
                CreatedOn = o.CreatedOn,
                DeletedOn = null,
                ETag = o.ETag.ToString(),
                FilesystemName = fileSystemClient.Name,
                LastModified = o.LastModified,
                Path = o.Name,
            });

        var batch = new List<PathRowType>(50000);


        var totalPathsProcessed = 0;
        var stopwatch = Stopwatch.StartNew();
        await using var timer = new Timer(s => { logger.LogInformation("Paths processed {count}... {dps} fps", totalPathsProcessed, totalPathsProcessed / (stopwatch.ElapsedMilliseconds / 1000f)); }, null, 3000, 3000);

        await foreach (var path in paths)
        {
            batch.Add(path);

            if (batch.Count == 50000)
            {
                await UpsertBatchAsync(batch, fileSystemClient);
                totalPathsProcessed += batch.Count;
                batch.Clear();
            }
        }

        if (batch.Any())
        {
            totalPathsProcessed += batch.Count;
            await UpsertBatchAsync(batch, fileSystemClient);
        }

        logger.LogInformation("Last batch sent, total paths processed {count}", totalPathsProcessed);
    }


    /// <summary>
    /// Upsert a batch and fetch metadata for modified files
    /// </summary>
    internal async Task UpsertBatchAsync(List<PathRowType> batch, DataLakeFileSystemClient fileSystemClient)
    {
        var modifiedPaths = await datalakeIndexer.UpsertPathsAsync(batch).ToListAsync();

        logger.LogInformation("Getting metadata for {count} modified paths", modifiedPaths.Count);
        var metadata = new ConcurrentBag<PathMetadataRowType>();
        await Parallel.ForEachAsync(modifiedPaths, new ParallelOptions { MaxDegreeOfParallelism = 256 }, async (path, token) =>
        {
            var props = await fileSystemClient.GetFileClient(path.Path).GetPropertiesAsync();

            metadata.Add(new PathMetadataRowType
            {
                MetadataJson = props.Value.Metadata.Any() ? JsonSerializer.Serialize(props.Value.Metadata) : null,
                PathKey = path.PathKey,
                ETag = path.ETag.ToString(),
            });
        });

        await datalakeIndexer.UpsertPathsMetadataAsync(metadata);
    }
}
