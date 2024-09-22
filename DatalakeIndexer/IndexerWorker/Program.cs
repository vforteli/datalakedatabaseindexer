#pragma warning disable CS8321 // Local function is declared but never used

using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.Json;
using System.Threading.Channels;
using Azure.Messaging.ServiceBus;
using Azure.Storage.Files.DataLake;
using Azure.Storage.Files.DataLake.Models;
using DatabaseIndexer;
using DataLakeFileSystemClientExtension;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;


var config = new ConfigurationBuilder().AddJsonFile("appsettings.json", true, true).AddEnvironmentVariables().Build();
var loggerFactory = LoggerFactory.Create(o => o.AddSimpleConsole(c => c.SingleLine = true));
var logger = loggerFactory.CreateLogger<Program>();

var queueName = config["QUEUE_NAME"];

var sqlConnectionFactory = new SqlConnectionFactory(config["SQL_CONNECTION_STRING"] ?? throw new ArgumentNullException("..."));
var datalakeIndexer = new DatalakeIndexer(sqlConnectionFactory, loggerFactory.CreateLogger<DatalakeIndexer>());

await using var serviceBusClient = new ServiceBusClient(config["SERVICEBUS_CONNECTION_STRING"]);

// await TestReceiveStuffAsync(logger, queueName, serviceBusClient);


// var receiver = serviceBusClient.CreateReceiver(queueName);
// while (true)
// {
//     logger.LogInformation("Waiting for messages on queue '{queue}'...", queueName);
//     var messages = await receiver.ReceiveMessagesAsync(5000, TimeSpan.FromSeconds(10));
//     if (messages.Count == 0)
//     {
//         break;
//     }

//     logger.LogInformation("Got batch with {count} messages", messages.Count);

//     var paths = messages.Select(o => o.Body.ToObjectFromJson<BlobEvent>()).Select(o =>
//     {
//         var (fileSystem, path) = Utils.UrlToFilesystemAndPath(o.Data.BlobUrl);

//         return new RowType
//         {
//             CreatedOn = o.EventTime,    // event time is not the actual created date.. but close enough for this purpose
//             DeletedOn = null,
//             FilesystemName = fileSystem,
//             LastModified = o.EventTime, // same here...
//             Path = path,
//         };
//     });

//     // dropping duplicates... occasionally due to sb shenanigans and batching we may end up with the same path in multiple messages
//     var pathsDuplicatesRemoved = paths
//         .GroupBy(o => new { o.FilesystemName, o.Path })
//         .Select(o => o.OrderByDescending(o => o.CreatedOn).First());

//     await datalakeIndexer.UpsertPathsAsync(pathsDuplicatesRemoved);
//     logger.LogInformation("Batch upserted to database");

//     // if we get here i suppose we can assume the upsert was successful and all messages in the batch were upserted... hopefully
//     await Parallel.ForEachAsync(messages, new ParallelOptions { MaxDegreeOfParallelism = 256 }, async (message, token) =>
//     {
//         await receiver.CompleteMessageAsync(message, token);
//     });

//     logger.LogInformation("Messages completed");
// }

// logger.LogInformation("No more messages received after 10 seconds, going back to sleep...");

await ListPathsAsync();


async Task ListPathsAsync()
{
    var dataLakeServiceClient = new DataLakeServiceClient(new Uri(config["DATALAKE_CONNECTION_STRING"] ?? throw new ArgumentNullException("...")));
    var fileSystemClient = dataLakeServiceClient.GetFileSystemClient("stuff-10m-files");

    var paths = fileSystemClient.ListPathsParallelAsync("")
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
            await UpsertBatchAsync(logger, datalakeIndexer, fileSystemClient, batch);
            totalPathsProcessed += batch.Count;
            batch.Clear();
        }
    }

    if (batch.Any())
    {
        totalPathsProcessed += batch.Count;
        await UpsertBatchAsync(logger, datalakeIndexer, fileSystemClient, batch);
    }

    logger.LogInformation("Last batch sent, total paths processed {count}", totalPathsProcessed);
}


static async Task TestReceiveStuffAsync(ILogger<Program> logger, string? queueName, ServiceBusClient serviceBusClient)
{
    var messageChannel = Channel.CreateBounded<ServiceBusReceivedMessage>(100000);
    var stopwatch = Stopwatch.StartNew();

    var receivedCount = 0;


    await Task.WhenAll(new List<Task> {
    ReceveStuffAsync(),
    // ReceveStuffAsync(),
    // ReceveStuffAsync(),
    // ReceveStuffAsync(),
});


    logger.LogInformation("Done after {elapsed}", stopwatch.ElapsedMilliseconds);


    async Task ReceveStuffAsync()
    {
        var receiver = serviceBusClient.CreateReceiver(queueName);
        while (true)
        {
            logger.LogInformation("Waiting for messages on queue '{queue}'...", queueName);
            var messages = await receiver.ReceiveMessagesAsync(1000, TimeSpan.FromSeconds(10));
            if (messages.Count == 0)
            {
                break;
            }

            logger.LogInformation("Got batch with {count} messages", messages.Count);

            foreach (var message in messages)
            {
                Interlocked.Increment(ref receivedCount);
                await messageChannel.Writer.WriteAsync(message);
            }

            logger.LogInformation("{count} messages received, rps: {rps}", receivedCount, receivedCount / stopwatch.Elapsed.TotalSeconds);
        }
    }
}

static async Task UpsertBatchAsync(ILogger<Program> logger, DatalakeIndexer datalakeIndexer, DataLakeFileSystemClient fileSystemClient, List<PathRowType> batch)
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