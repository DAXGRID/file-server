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
            .Select(x => new FileInfo(x))
            // We do not want hidden files or directories to be shown.
            .Where(x => !x.Name.StartsWith('.'))
            .Select(
                x =>
                {
                    return IsDirectory(x.FullName)
                        ? FormatDirectoryEntry(x.Name, Path.Combine(route, x.Name), x.LastWriteTime, true)
                        : FormatFileEntry(x.Name, route, x.LastWriteTime, FileSizeFormat.SizeSuffix(x.Length));
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
        var resourceLink = Path.Combine(route, name);
        return $"<li><a href=\"/{resourceLink}\"><p>{name}</p><p>{fileSize}</p><p>{lastModified.ToString("g", new CultureInfo("en-gb"))}</p></a>{DeleteForm(resourceLink)}</li>";
    }

    private static string DeleteForm(string actionLink)
    {
        return $"<form style=\"align-self: center; padding: 0px 5px;\" action=\"/{actionLink}?redirect\" method=\"post\"><input type=\"hidden\" name=\"_method\" value=\"DELETE\"> <button type=\"submit\">Delete</button></form>";
    }
}
