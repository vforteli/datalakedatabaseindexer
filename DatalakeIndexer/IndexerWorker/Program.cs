#pragma warning disable CS8321 // Local function is declared but never used
#pragma warning disable CS0219 // Variable is assigned but its value is never used

using System.Diagnostics;
using System.Threading.Channels;
using Azure.Messaging.ServiceBus;
using Azure.Storage.Files.DataLake;
using DatabaseIndexer;
using IndexerWorker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;


var config = new ConfigurationBuilder().AddJsonFile("appsettings.json", true, true).AddEnvironmentVariables().Build();
var loggerFactory = LoggerFactory.Create(o => o.AddSimpleConsole(c => c.SingleLine = true));
var logger = loggerFactory.CreateLogger<Program>();

var queueName = config["QUEUE_NAME"];
var filesystemName = "stuff-large-files";

var sqlConnectionFactory = new SqlConnectionFactory(config["SQL_CONNECTION_STRING"] ?? throw new ArgumentNullException("..."));
var datalakeIndexer = new SqlServerIndexer(sqlConnectionFactory, loggerFactory.CreateLogger<SqlServerIndexer>());
var dataLakeServiceClient = new DataLakeServiceClient(new Uri(config["DATALAKE_CONNECTION_STRING"] ?? throw new ArgumentNullException("...")));
await using var serviceBusClient = new ServiceBusClient(config["SERVICEBUS_CONNECTION_STRING"]);

var indexer = new Indexer(
    loggerFactory.CreateLogger<Indexer>(),
    datalakeIndexer,
    dataLakeServiceClient);





await TestReceiveStuffAsync(logger, queueName, serviceBusClient);


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

//         return new PathRowType
//         {
//             CreatedOn = o.EventTime,    // event time is not the actual created date.. but close enough for this purpose
//             DeletedOn = null,
//             FilesystemName = fileSystem,
//             LastModified = o.EventTime, // same here...
//             Path = path,
//             ETag = "",
//         };
//     });

//     // dropping duplicates... occasionally due to sb shenanigans and batching we may end up with the same path in multiple messages
//     var pathsDuplicatesRemoved = paths
//         .GroupBy(o => new { o.FilesystemName, o.Path })
//         .Select(o => o.OrderByDescending(o => o.CreatedOn).First());

//     // await datalakeIndexer.UpsertPathsAsync(pathsDuplicatesRemoved);
//     logger.LogInformation("Batch upserted to database");

//     // if we get here i suppose we can assume the upsert was successful and all messages in the batch were upserted... hopefully
//     await Parallel.ForEachAsync(messages, new ParallelOptions { MaxDegreeOfParallelism = 256 }, async (message, token) =>
//     {
//         await receiver.CompleteMessageAsync(message, token);
//     });

//     logger.LogInformation("Messages completed");
// }

// logger.LogInformation("No more messages received after 10 seconds, going back to sleep...");

// await indexer.ListPathsAsync(filesystemName);



static async Task TestReceiveStuffAsync(ILogger<Program> logger, string? queueName, ServiceBusClient serviceBusClient)
{
    var messageChannel = Channel.CreateBounded<ServiceBusReceivedMessage>(100000);
    var stopwatch = Stopwatch.StartNew();

    var receivedCount = 0;


    await Task.WhenAll(new List<Task>
    {
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
