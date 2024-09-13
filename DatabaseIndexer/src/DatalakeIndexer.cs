using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace DatabaseIndexer;

public class DatalakeIndexer(SqlConnection connection, ILogger<DatalakeIndexer> logger)
{
    public async Task<int> UpsertPathsAsync(IEnumerable<RowType> paths)
    {
        await connection.OpenAsync();

        var totalRowsAffected = 0;
        foreach (var chunk in paths.Chunk(100000))
        {
            totalRowsAffected += await UpsertBatchAsync(chunk);
            logger.LogInformation("Total rows affected {rows}", totalRowsAffected);
        }

        return totalRowsAffected;
    }


    internal async Task<int> UpsertBatchAsync(IEnumerable<RowType> rows)
    {
        await new SqlCommand("""SELECT TOP(0) * INTO #paths FROM Paths""", connection).ExecuteNonQueryAsync();

        using var bulk = new SqlBulkCopy(connection)
        {
            DestinationTableName = "#paths",
            BatchSize = 10000,
            NotifyAfter = 10000,
            BulkCopyTimeout = 300,
        };

        // just dump every property into columns and hope they match the db
        typeof(RowType).GetProperties().Select(o => o.Name).ToList().ForEach(p =>
        {
            bulk.ColumnMappings.Add(p, p);
        });

        bulk.SqlRowsCopied += (_, e) =>
        {
            logger.LogDebug("Bulk loaded {rows}", e.RowsCopied);
        };

        await bulk.WriteToServerAsync(new GenericDataReader<RowType>(rows));


        logger.LogInformation("Bulk loaded {rows} into temp table", bulk.RowsCopied);

        var rowsAffected = await new SqlCommand("""
            MERGE Paths AS target USING #paths AS source
            ON 
                target.FilesystemName = source.FilesystemName 
                AND target.Path = source.Path

            WHEN NOT MATCHED BY TARGET THEN
                INSERT (FilesystemName, Path, CreatedOn, LastModified, DeletedOn)
                VALUES (source.FilesystemName, source.Path, source.CreatedOn, source.LastModified, source.DeletedOn)  
                
            WHEN MATCHED THEN            
                UPDATE SET 
                    target.CreatedOn = source.CreatedOn,
                    target.LastModified = source.LastModified,
                    target.DeletedOn = source.DeletedOn

            ;  

            DROP TABLE #paths
            """, connection)
        {
            CommandTimeout = 300
        }.ExecuteNonQueryAsync();

        logger.LogInformation("Merged {rows} from temp table", rowsAffected);

        return rowsAffected;
    }
}

