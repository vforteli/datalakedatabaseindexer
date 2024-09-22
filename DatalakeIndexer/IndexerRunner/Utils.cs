namespace DatabaseIndexer;

public class Utils
{
    /// <summary>
    /// Get some paths for testing
    /// </summary>
    /// <returns></returns>
    public static IEnumerable<PathRowType> GetMockPaths(int fileSystemCount = 10, int directoryCount = 100, int fileCount = 100)
    {
        var filesystems = Enumerable.Range(0, fileSystemCount).Select(o => $"somefilesystem_{o}");

        // var extensions = new List<string> { "json", "pdf", "txt", "exe" };

        // string GetRandomFilePath() => extensions[Random.Shared.Next(0, extensions.Count)];

        var directories = Enumerable.Range(0, directoryCount).Select(o => $"somedirectory_{o}");

        var paths = Enumerable.Range(0, fileCount).Select(o => $"somedirectory/somepath_{o}");

        var now = DateTime.UtcNow;

        var fullPaths = directories
            .SelectMany((d, i) => paths.Select(p => $"{d}/{p}.json")
            .Select(o => new PathRowType
            {
                FilesystemName = "somefilesystem",
                Path = o,
                CreatedOn = now.AddDays(-i),
                LastModified = now,
                DeletedOn = null,
                ETag = "0x8DCD67FE7D7C666"
            }));

        return fullPaths;
    }
}
