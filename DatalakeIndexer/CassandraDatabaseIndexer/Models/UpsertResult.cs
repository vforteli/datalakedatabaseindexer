namespace DatabaseIndexer;

public record UpsertResult<T>(int UpdateCount, int InsertCount, int ProcessedCount, List<T> ModifiedRows);
