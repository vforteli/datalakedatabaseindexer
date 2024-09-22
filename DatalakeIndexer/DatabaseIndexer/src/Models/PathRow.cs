using System.Security.Cryptography;
using System.Text;

namespace DatabaseIndexer;

public record PathRowType
{
    required public string FilesystemName { get; init; }
    required public string Path { get; init; }
    required public DateTimeOffset? CreatedOn { get; init; }
    required public DateTimeOffset? LastModified { get; init; }
    required public DateTimeOffset? DeletedOn { get; init; }
    public byte[] PathKey => SHA256.HashData(Encoding.UTF8.GetBytes(FilesystemName + Path));
    required public string ETag { get; init; }
}

public record PathMetadataRowType
{
    required public byte[] PathKey { get; init; }
    required public string? MetadataJson { get; init; }
    required public string ETag { get; init; }
}