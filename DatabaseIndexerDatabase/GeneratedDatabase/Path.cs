using System;
using System.Collections.Generic;

namespace DatabaseIndexerDatabase.GeneratedDatabase;

public partial class Path
{
    public long PathId { get; set; }

    public string FilesystemName { get; set; } = null!;

    public string Path1 { get; set; } = null!;

    public string? PathReversed { get; set; }

    public DateTimeOffset? CreatedOn { get; set; }

    public DateTimeOffset? LastModified { get; set; }

    public DateTimeOffset? DeletedOn { get; set; }
}
