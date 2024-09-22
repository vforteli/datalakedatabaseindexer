using System;
using System.Collections.Generic;

namespace DatabaseIndexerDatabase.GeneratedDatabase;

public partial class Paths
{
    public byte[] PathKey { get; set; } = null!;

    public string FilesystemName { get; set; } = null!;

    public string Path { get; set; } = null!;

    public string? Path_reversed { get; set; }

    public DateTimeOffset? CreatedOn { get; set; }

    public DateTimeOffset? LastModified { get; set; }

    public DateTimeOffset? DeletedOn { get; set; }

    public string? ETag { get; set; }
}
