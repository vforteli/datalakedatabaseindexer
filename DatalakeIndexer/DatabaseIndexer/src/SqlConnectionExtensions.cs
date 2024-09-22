using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace DatabaseIndexer;

public static class SqlConnectionExteions
{
    /// <summary>
    /// Bulk load some IEnumerable into specified temp table
    /// All properties will be inserted into columns with matching names
    /// </summary>
    internal static async Task<int> BulkLoadAsync<T>(this SqlConnection connection, ILogger<DatalakeIndexer> logger, IEnumerable<T> rows, string tableName, int batchSize)
    {
        using var bulk = new SqlBulkCopy(connection)
        {
            DestinationTableName = tableName,
            BatchSize = batchSize,
            NotifyAfter = 10000,
            BulkCopyTimeout = 300,
        };

        // just dump every property into columns and hope they match the db
        typeof(T).GetProperties().Select(o => o.Name).ToList().ForEach(p =>
        {
            bulk.ColumnMappings.Add(p, p);
        });

        bulk.SqlRowsCopied += (_, e) =>
        {
            logger.LogDebug("Bulk loaded {rows}", e.RowsCopied);
        };

        await bulk.WriteToServerAsync(new GenericDataReader<T>(rows));

        logger.LogDebug("Bulk loaded {rows} into {tablename} table", bulk.RowsCopied, tableName);

        return bulk.RowsCopied;
    }
}