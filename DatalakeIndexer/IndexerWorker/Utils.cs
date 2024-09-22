public class Utils
{
    public static (string fileSystem, string path) UrlToFilesystemAndPath(string url)
    {
        var parts = url.Split('/', 5);
        return (parts[3], parts[4]);
    }
}