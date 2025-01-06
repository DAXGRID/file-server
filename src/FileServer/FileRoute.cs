namespace FileServer;

internal static class FileRoute
{
    public static void Setup(WebApplication app, ILoggerFactory loggerFactory)
    {
        var logger = loggerFactory.CreateLogger(nameof(FileRoute));

        app.MapGet(
            "/{*rest}",
            (string route) =>
            {
                logger.LogInformation("Requested {FileName}...", route);
                return Results.Ok(route);
            }
        );
    }
}
