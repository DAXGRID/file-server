namespace FileServer;

internal static class FileRoute
{
    public static void Setup(WebApplication app, ILoggerFactory loggerFactory)
    {
        var logger = loggerFactory.CreateLogger(nameof(FileRoute));

        app.MapGet(
            $"/",
            () =>
            {
                logger.LogInformation("Requested file...");
                return Results.Ok("Hello, World!");
            }
        );
    }
}
