using System.Diagnostics;
using Dapper;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace PGDatabaseIndexer;

public class PGServerIndexer(NpgsqlDataSource dataSource, ILogger<PGServerIndexer> logger)
{
    /// <summary>
    /// Upsert paths.
    /// Returns the modified paths so that metadata can be retrieved only for the relevant rows
    /// </summary>
    public async IAsyncEnumerable<PathRowType> UpsertPathsAsync(IEnumerable<PathRowType> paths, int chunkSize = 100000)
    {
        var totalInserts = 0;
        var totalUpdates = 0;
        var totalRowsProcessed = 0;

        var stopwatch = Stopwatch.StartNew();
        foreach (var chunk in paths.Chunk(chunkSize))
        {
            var result = await UpsertPathsBatchAsync(chunk);
            totalInserts += result.InsertCount;
            totalUpdates += result.UpdateCount;
            totalRowsProcessed += result.ProcessedCount;
            var seconds = stopwatch.Elapsed.TotalSeconds;

            logger.LogInformation(
                "Total rows inserts: {totalInserts}, updates: {totalUpdates}, processed: {processed} after {elapsed}, rps: {rps}",
                totalInserts,
                totalUpdates,
                totalRowsProcessed,
                seconds,
                seconds > 0 ? (totalRowsProcessed / seconds) : 0);

            foreach (var path in result.ModifiedRows)
            {
                yield return path;
            }
        }
    }


    internal async Task<UpsertResult<PathRowTypeUpsert>> UpsertPathsBatchAsync(IEnumerable<PathRowType> rows)
    {
        await using var connection = dataSource.CreateConnection();
        await connection.OpenAsync();

        await connection.ExecuteScalarAsync(
            "CREATE TEMP TABLE IF NOT EXISTS paths_temp AS SELECT * FROM paths LIMIT 0;");

        var rowsCopied = await connection.BulkLoadAsync(logger, rows, "paths_temp");

        var affectedRows = (await connection.QueryAsync<PathRowTypeUpsert>(
            """
            MERGE INTO paths
            USING paths_temp
            ON paths.path_key = paths_temp.path_key
            WHEN MATCHED AND paths.etag != paths_temp.etag THEN
                UPDATE SET 
                    etag = paths_temp.etag,
                    created_on = paths_temp.created_on, 
                    last_modified = paths_temp.last_modified, 
                    deleted_on = paths_temp.deleted_on,
                    should_update_metadata = True

            WHEN NOT MATCHED THEN
                INSERT (filesystem_name, path, created_on, last_modified, deleted_on, path_key, etag)
                VALUES (
                    paths_temp.filesystem_name, 
                    paths_temp.path, 
                    paths_temp.created_on,
                    paths_temp.last_modified, 
                    paths_temp.deleted_on, 
                    paths_temp.path_key,
                    paths_temp.etag)

            RETURNING
                merge_action() as action, paths_temp.*
            ;
            DROP TABLE paths_temp;
            """)).AsList();

        var updateCount = affectedRows.Count(o => o.action == "UPDATE");
        var insertCount = affectedRows.Count(o => o.action == "INSERT");
        var totalRowsAffected = updateCount + insertCount;

        logger.LogInformation("Upserted {rows} into paths. Inserts: {inserts}, updates: {updates}",
            totalRowsAffected,
            insertCount,
            updateCount);

        return new UpsertResult<PathRowTypeUpsert>(updateCount, insertCount, (int)rowsCopied, affectedRows);
    }


    /// <summary>
    /// Upsert path metadata and update ETag of modified paths
    /// </summary>
    public async Task<int> UpsertPathsMetadataAsync(IEnumerable<PathMetadataRowType> paths, int chunkSize = 50000)
    {
        await using var connection = dataSource.CreateConnection();
        await connection.OpenAsync();

        var totalRowsAffected = 0;

        var stopwatch = Stopwatch.StartNew();
        foreach (var chunk in paths.Chunk(chunkSize))
        {
            totalRowsAffected += await UpsertPathsMetadataBatchAsync(connection, chunk);
            var seconds = stopwatch.Elapsed.TotalSeconds;

            logger.LogInformation(
                "Total rows affected {rows} after {elapsed}, rps: {rps}",
                totalRowsAffected,
                seconds,
                seconds > 0 ? (totalRowsAffected / seconds) : 0);
        }

        return totalRowsAffected;
    }


    internal async Task<int> UpsertPathsMetadataBatchAsync(
        NpgsqlConnection connection,
        IEnumerable<PathMetadataRowType> rows)
    {
        await connection.ExecuteScalarAsync(
            """
            CREATE TEMP TABLE metadata_temp (
                path_key bytea NOT NULL,
                etag character varying(20) NOT NULL,
                metadata_json varchar(4096)
            );
            """);

        await connection.BulkLoadAsync(logger, rows, "metadata_temp");

        var affectedRows = await connection.ExecuteAsync(
            """
            UPDATE paths
            SET 
                metadata_json = metadata_temp.metadata_json::jsonb,
                should_update_metadata = False
            FROM metadata_temp
            WHERE paths.path_key = metadata_temp.path_key;

            DROP TABLE metadata_temp;
            """);

        logger.LogInformation("Updated {rows} in metadata", affectedRows);

        return affectedRows;
    }
}