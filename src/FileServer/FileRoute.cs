using static FileServer.FileServerDirectory;

namespace FileServer;

internal static class FileRoute
{
    public static void Setup(WebApplication app, ILoggerFactory loggerFactory, Settings settings)
    {
        var logger = loggerFactory.CreateLogger(nameof(FileRoute));

        var userDirectoryPathsLookup = settings.FileServerUsers.ToDictionary(x => x.Username, x => x.FolderPath)
            ?? throw new InvalidOperationException("Could not convert file server users to a directory path lookup.");

        // Special route to get the favicon.
        app.MapGet("/favicon.ico", () => {
            return Results.File(File.ReadAllBytes("Site/favicon.ico"), "image/x-icon");
        });

        app.MapGet(
            "{*route}",
            (HttpContext context, string route = "") =>
            {
                ArgumentNullException.ThrowIfNull(
                    context.User?.Identity?.Name,
                    "The user identity was not set, something is wrong with the authentication middleware.");

                logger.LogInformation("Requested {FileName}...", route);

                if (!userDirectoryPathsLookup.TryGetValue(context.User.Identity.Name, out string? userPath))
                {
                    throw new InvalidOperationException($"Could not get user path for user: {context.User.Identity.Name}");
                }

                var fileSystemEntryPath = Path.Combine(userPath, route);
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
