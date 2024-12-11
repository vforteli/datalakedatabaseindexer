using System;
using System.Collections.Generic;

namespace PGDatabaseContext.GeneratedDatabase;

public partial class paths
{
    public byte[] path_key { get; set; } = null!;

    public string filesystem_name { get; set; } = null!;

    public string path { get; set; } = null!;

    public string? path_reversed { get; set; }

    public DateTime? created_on { get; set; }

    public DateTime? last_modified { get; set; }

    public DateTime? deleted_on { get; set; }

    public string etag { get; set; } = null!;

    public string? metadata_json { get; set; }

    public bool should_update_metadata { get; set; }
}
