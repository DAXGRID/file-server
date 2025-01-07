using static FileServer.FileServerDirectory;

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
                    var fileLinks = HtmlDirectoryBuilder
                        .CreateDirectoryLinkStructure(fileSystemEntryPath, route);

                    var indexFile = File.ReadAllText("Site/index.html")
                        .Replace("{{ Files }}", fileLinks, StringComparison.InvariantCulture);

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
}
