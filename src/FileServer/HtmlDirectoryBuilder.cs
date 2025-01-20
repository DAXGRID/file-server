using static FileServer.FileServerDirectory;
using System.Globalization;

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
                {
                    var fileName = Path.GetFileName(x);
                    var fileInfo = new FileInfo(x);

                    return IsDirectory(x)
                        ? FormatDirectoryEntry(fileName, Path.Combine(route, fileName), fileInfo.LastWriteTime)
                        : FormatFileEntry(Path.GetFileName(x), route, fileInfo.LastWriteTime, FileSizeFormat.SizeSuffix(fileInfo.Length));
                }
            ).ToList();

        if (!string.IsNullOrWhiteSpace(route))
        {
            var previousPath = route.LastIndexOf('/') == -1 ? "" : $"{route.Substring(0, route.LastIndexOf('/'))}";
            fileLinks.Insert(0, FormatDirectoryEntry("..", previousPath, null));
        }

        return $"<ul>{string.Join("", fileLinks)}</ul>";
    }

    private static string FormatDirectoryEntry(string name, string route, DateTime? lastModified)
    {
        return $"<li><a style=\"font-weight: bold\" href=\"/{route}\"><p>{name}/</p><p></p><p>{lastModified?.ToString("g", new CultureInfo("en-gb")) ?? ""}</p></a></li>";
    }

    private static string FormatFileEntry(string name, string route, DateTime lastModified, string fileSize)
    {
        return $"<li><a href=\"/{route}/{name}\"><p>{name}</p><p>{fileSize}</p><p>{lastModified.ToString("g", new CultureInfo("en-gb"))}</p></a>{DeleteFile($"{route}/{name}")}</li>";
    }

    private static string DeleteFile(string actionLink)
    {
        return $"<form style=\"align-self: center; padding: 0px 5px;\" action=\"/{actionLink}?redirect\" method=\"post\"><input type=\"hidden\" name=\"_method\" value=\"DELETE\"> <button type=\"submit\">Delete</button></form>";
    }
}
