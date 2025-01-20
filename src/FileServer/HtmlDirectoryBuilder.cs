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
                        ? FormatDirectoryEntry(fileName, Path.Combine(route, fileName), fileInfo.LastWriteTime, true)
                        : FormatFileEntry(Path.GetFileName(x), route, fileInfo.LastWriteTime, FileSizeFormat.SizeSuffix(fileInfo.Length));
                }
            ).ToList();

        if (!string.IsNullOrWhiteSpace(route))
        {
            // This is done when URL's are ending in /
            var trimmedRoute = route.Last() == '/' ? route.Remove(route.Length - 1, 1) : route;
            var previousPath = trimmedRoute.LastIndexOf('/') == -1 ? "" : $"{trimmedRoute.Substring(0, trimmedRoute.LastIndexOf('/'))}";
            fileLinks.Insert(0, FormatDirectoryEntry("..", previousPath, null, false));
        }

        return $"<ul>{string.Join("", fileLinks)}</ul>";
    }

    private static string FormatDirectoryEntry(string name, string route, DateTime? lastModified, bool allowDelete)
    {
        var deleteForm = allowDelete ? DeleteForm($"{route}") : "";
        return $"<li><a style=\"font-weight: bold\" href=\"/{route}\"><p>{name}/</p><p></p><p>{lastModified?.ToString("g", new CultureInfo("en-gb")) ?? ""}</p></a>{deleteForm}</li>";
    }

    private static string FormatFileEntry(string name, string route, DateTime lastModified, string fileSize)
    {
        var deleteFileActionPath = Path.Combine(route, name);
        var linkPath = Path.Combine(route, name);
        return $"<li><a href=\"/{linkPath}\"><p>{name}</p><p>{fileSize}</p><p>{lastModified.ToString("g", new CultureInfo("en-gb"))}</p></a>{DeleteForm(deleteFileActionPath)}</li>";
    }

    private static string DeleteForm(string actionLink)
    {
        return $"<form style=\"align-self: center; padding: 0px 5px;\" action=\"/{actionLink}?redirect\" method=\"post\"><input type=\"hidden\" name=\"_method\" value=\"DELETE\"> <button type=\"submit\">Delete</button></form>";
    }
}
