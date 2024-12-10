using System.Security.Cryptography;
using System.Text;

namespace PGDatabaseIndexer;

public record PathRowType
{
    public required string filesystem_name { get; init; }
    public required string path { get; init; }
    public required DateTimeOffset? created_on { get; init; }
    public required DateTimeOffset? last_modified { get; init; }
    public required DateTimeOffset? deleted_on { get; init; }
    public byte[] path_key => SHA256.HashData(Encoding.UTF8.GetBytes(filesystem_name + path));
    public required string etag { get; init; }
}

public record PathRowTypeUpsert : PathRowType
{
    public required string action { get; init; }
}

public record PathMetadataRowType
{
    public required byte[] path_key { get; init; }
    public required string? metadata_json { get; init; }
    public required string etag { get; init; }
}