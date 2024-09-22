using System.Diagnostics;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace DatabaseIndexer;

public class DatalakeIndexer(ISqlConnectionFactory sqlConnectionFactory, ILogger<DatalakeIndexer> logger)
{
    /// <summary>
    /// Upsert paths.
    /// Returns the modified paths so that metadata can be retrieved only for the relevant rows
    /// </summary>
    public async IAsyncEnumerable<PathRowType> UpsertPathsAsync(IEnumerable<PathRowType> paths, int chunkSize = 50000)
    {
        using var connection = sqlConnectionFactory.Create();
        await connection.OpenAsync();

        var totalRowsAffected = 0;
        var totalRowsProcessed = 0;

        var stopwatch = Stopwatch.StartNew();
        foreach (var chunk in paths.Chunk(50000))
        {
            var result = await UpsertPathsBatchAsync(connection, chunk, chunkSize);
            totalRowsAffected += result.InsertCount + result.UpdateCount;
            totalRowsProcessed += result.ProcessedCount;
            var seconds = stopwatch.Elapsed.TotalSeconds;

            logger.LogInformation(
                "Total rows affected: {rows}, processed: {processed} after {elapsed}, rps: {rps}",
                totalRowsAffected,
                totalRowsProcessed,
                seconds,
                seconds > 0 ? (totalRowsProcessed / seconds) : 0);

            foreach (var path in result.ModifiedRows)
            {
                yield return path;
            }
        }
    }


    /// <summary>
    /// Upsert path metadata and update ETag of modified paths
    /// </summary>
    public async Task<int> UpsertPathsMetadataAsync(IEnumerable<PathMetadataRowType> paths, int chunkSize = 50000)
    {
        using var connection = sqlConnectionFactory.Create();
        await connection.OpenAsync();

        var totalRowsAffected = 0;

        var stopwatch = Stopwatch.StartNew();
        foreach (var chunk in paths.Chunk(50000))
        {
            totalRowsAffected += await UpsertPathsMetadataBatchAsync(connection, chunk, chunkSize);
            var seconds = stopwatch.Elapsed.TotalSeconds;

            logger.LogInformation(
                "Total rows affected {rows} after {elapsed}, rps: {rps}",
                totalRowsAffected,
                seconds,
                seconds > 0 ? (totalRowsAffected / seconds) : 0);
        }

        return totalRowsAffected;
    }


    internal async Task<UpsertResult<PathRowType>> UpsertPathsBatchAsync(SqlConnection connection, IEnumerable<PathRowType> rows, int batchSize)
    {
        await connection.ExecuteScalarAsync("""
            SELECT TOP(0) * INTO #paths FROM Paths
            ALTER TABLE #paths ADD IsInsert BIT            
            """);

        var rowsCopied = await connection.BulkLoadAsync(logger, rows, "#paths", batchSize);

        var result = await connection.QueryMultipleAsync("""
            UPDATE #paths SET IsInsert = 1 WHERE PathKey NOT IN (SELECT PathKey FROM Paths)
            
            INSERT INTO Paths (FilesystemName, Path, CreatedOn, LastModified, DeletedOn, PathKey)
                SELECT temp.FilesystemName, temp.Path, temp.CreatedOn, temp.LastModified, temp.DeletedOn, temp.PathKey
                FROM #paths AS temp
                WHERE IsInsert = 1

            DECLARE @insertCount INT = @@ROWCOUNT


            UPDATE paths SET 
                paths.CreatedOn = temp.CreatedOn,
                paths.LastModified = temp.LastModified,
                paths.DeletedOn = temp.DeletedOn
            FROM Paths AS paths
            INNER JOIN #paths AS temp ON paths.PathKey = temp.PathKey
            WHERE temp.IsInsert IS NULL AND paths.ETag != temp.ETag

            DECLARE @updateCount INT = @@ROWCOUNT


            SELECT @updateCount AS UpdateCount, @insertCount AS InsertCount

            SELECT temp.FilesystemName, temp.Path, temp.CreatedOn, temp.LastModified, temp.DeletedOn, temp.ETag 
            FROM #paths AS temp
            LEFT JOIN Paths AS paths ON temp.PathKey = paths.PathKey
            WHERE temp.IsInsert = 1 OR paths.ETag IS NULL OR temp.ETag != paths.ETag

            DROP TABLE #paths
            """);

        var (updateCount, insertCount) = await result.ReadSingleOrDefaultAsync<(int updateCount, int insertCount)>();
        var totalRowsAffected = updateCount + insertCount;

        var modifiedRows = await result.ReadAsync<PathRowType>();

        logger.LogInformation("Upserted {rows} into paths. Inserts: {inserts}, updates: {updates}", totalRowsAffected, insertCount, updateCount);

        return new UpsertResult<PathRowType>(updateCount, insertCount, rowsCopied, modifiedRows.AsList());
    }


    internal async Task<int> UpsertPathsMetadataBatchAsync(SqlConnection connection, IEnumerable<PathMetadataRowType> rows, int batchSize)
    {
        await connection.ExecuteScalarAsync("""
            SELECT TOP(0) * INTO #pathsmetadata FROM PathsMetadata
            ALTER TABLE #pathsmetadata ADD IsUpdate BIT
            ALTER TABLE #pathsmetadata ADD ETag nvarchar(20)
            """);

        await connection.BulkLoadAsync(logger, rows, "#pathsmetadata", batchSize);

        var (updateCount, insertCount) = await connection.QuerySingleOrDefaultAsync<(int updateCount, int insertCount)>("""
            UPDATE #pathsmetadata SET IsUpdate = 1 WHERE PathKey IN (SELECT PathKey FROM PathsMetadata)
            
            INSERT INTO PathsMetadata (PathKey, MetadataJson)
                SELECT s.PathKey, s.MetadataJson
                FROM #pathsmetadata AS s               
                WHERE IsUpdate IS NULL AND s.MetadataJson IS NOT NULL
            
            DECLARE @insertCount INT = @@ROWCOUNT

            
            UPDATE metadata SET 
                metadata.MetadataJson = temp.MetadataJson              
            FROM PathsMetadata AS metadata
            INNER JOIN #pathsmetadata AS temp ON metadata.PathKey = temp.PathKey
            WHERE temp.IsUpdate = 1

            DECLARE @updateCount INT = @@ROWCOUNT       

            UPDATE paths SET 
                paths.ETag = temp.ETag              
            FROM Paths paths
            INNER JOIN #pathsmetadata AS temp ON temp.PathKey = paths.PathKey

            DROP TABLE #pathsmetadata

            SELECT @updateCount AS UpdateCount, @insertCount AS InsertCount
            """);

        var totalRowsAffected = updateCount + insertCount;

        logger.LogInformation("Upserted {rows} into metadata. Inserts: {inserts}, updates: {updates}", totalRowsAffected, insertCount, updateCount);

        return totalRowsAffected;
    }
}
