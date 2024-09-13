namespace DatabaseIndexer;

public record RowType
{
    required public string FilesystemName { get; init; }
    required public string Path { get; init; }
    required public DateTimeOffset? CreatedOn { get; init; }
    required public DateTimeOffset? LastModified { get; init; }
    required public DateTimeOffset? DeletedOn { get; init; }
}