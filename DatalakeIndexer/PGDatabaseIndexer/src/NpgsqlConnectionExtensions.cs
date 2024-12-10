using DatabaseIndexer;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace PGDatabaseIndexer;

public static class NpgsqlConnectionExtensions
{
    /// <summary>
    /// Bulk load some IEnumerable into specified temp table
    /// All properties will be inserted into columns with matching names
    /// </summary>
    internal static async Task<ulong> BulkLoadAsync<T>(
        this NpgsqlConnection connection,
        ILogger<PGServerIndexer> logger,
        IEnumerable<T> rows,
        string tableName)
    {
        var properties = typeof(T).GetProperties().Select(o => o.Name).ToList();

        await using var writer = await connection.BeginBinaryImportAsync(
            $"COPY {tableName} ({string.Join(", ", properties)}) FROM STDIN (FORMAT BINARY)").ConfigureAwait(false);

        var dataReader = new GenericDataReader<T>(rows);
        while (dataReader.Read())
        {
            await writer.StartRowAsync().ConfigureAwait(false);
            for (var i = 0; i < dataReader.FieldCount; i++)
            {
                var value = dataReader.GetValue(i);
                await writer.WriteAsync(value).ConfigureAwait(false);
            }
        }

        var rowsCopied = await writer.CompleteAsync();
        logger.LogDebug("Bulk loaded {rows} into {tablename} table", rowsCopied, tableName);
        return rowsCopied;
    }
}