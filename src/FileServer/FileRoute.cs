namespace FileServer;

internal static class FileRoute
{
    public static void Setup(WebApplication app, ILoggerFactory loggerFactory)
    {
        const string defaultPath = "";
        var logger = loggerFactory.CreateLogger(nameof(FileRoute));

        app.MapGet(
            "{*route}",
            (string route = "") =>
            {
                logger.LogInformation("Requested {FileName}...", route);

                var fileSystemEntryPath = Path.Combine(defaultPath, route);
                var fileExists = File.Exists(fileSystemEntryPath);

                if (!fileExists)
                {
                    Results.NotFound();
                }

                if (IsDirectory(fileSystemEntryPath))
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

                    var indexFile = File.ReadAllText("Site/index.html")
                        .Replace("{{ Files }}", string.Join("</br>", fileLinks), StringComparison.InvariantCulture);

                    return Results.Content(
                        indexFile,
                        "text/html");
                }
                else
                {
                    return Results.File(fileSystemEntryPath);
                }
            }
        );
    }

    private static string FormatDirectoryEntry(string name, string route)
    {
        return $"<a href=\"{route}/{name}\">{name}/</a>";
    }

    private static string FormatFileEntry(string name, string route)
    {
        return $"<a href=\"{route}/{name}\">{name}</a>";
    }

    private static bool IsDirectory(string filePath)
    {
        return File.GetAttributes(filePath).HasFlag(FileAttributes.Directory);
    }
}
