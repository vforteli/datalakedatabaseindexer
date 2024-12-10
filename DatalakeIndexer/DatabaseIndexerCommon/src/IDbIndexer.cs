namespace DatabaseIndexer;

public interface IDbIndexer
{
    /// <summary>
    /// Upsert paths.
    /// Returns the modified paths so that metadata can be retrieved only for the relevant rows
    /// </summary>
    public IAsyncEnumerable<PathRowType> UpsertPathsAsync(IEnumerable<PathRowType> paths, int chunkSize = 50000);

    /// <summary>
    /// Upsert path metadata and update ETag of modified paths
    /// </summary>
    public Task<int> UpsertPathsMetadataAsync(IEnumerable<PathMetadataRowType> paths, int chunkSize = 50000);
}