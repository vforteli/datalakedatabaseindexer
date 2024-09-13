namespace DatabaseIndexer;

public class Utils
{
    /// <summary>
    /// Get some paths for testing
    /// </summary>
    /// <returns></returns>
    public static IEnumerable<RowType> GetMockPaths()
    {
        var filesystems = Enumerable.Range(0, 10).Select(o => $"somefilesystem_{o}");

        var extensions = new List<string> { "json", "pdf", "txt", "exe" };

        var directories = Enumerable.Range(0, 1000).Select(o => $"somedirectory_{o}");

        var paths = Enumerable.Range(0, 10000).Select(o => $"somedirectory/somepath_{o}");

        var now = DateTime.UtcNow;

        var fullPaths = directories
            .SelectMany((d, i) => paths.Select(p => $"{d}/{p}.{extensions[Random.Shared.Next(0, extensions.Count)]}")
            .Select(o => new RowType
            {
                FilesystemName = "somefilesystem",
                Path = o,
                CreatedOn = now.AddDays(-i),
                LastModified = now,
                DeletedOn = null,
            }));

        return fullPaths;
    }
}
