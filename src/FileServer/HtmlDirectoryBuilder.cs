using static FileServer.FileServerDirectory;

namespace FileServer;

internal static class HtmlDirectoryBuilder
{
    public static string CreateDirectoryLinkStructure(
        string fileSystemEntryPath,
        string route)
    {
        var fileLinks = Directory
            .EnumerateFileSystemEntries(fileSystemEntryPath)
            .OrderByDescending(x => IsDirectory(x))
            .ThenBy(x => x)
            .Select(
                x =>
                IsDirectory(x)
                ? FormatDirectoryEntry(Path.GetFileName(x), route)
                : FormatFileEntry(Path.GetFileName(x), route)
            );

        return string.Join("</br>", fileLinks);
    }

    private static string FormatDirectoryEntry(string name, string route)
    {
        return $"<a href=\"{route}/{name}\">{name}/</a>";
    }

    private static string FormatFileEntry(string name, string route)
    {
        return $"<a href=\"{route}/{name}\">{name}</a>";
    }
}