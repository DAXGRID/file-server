namespace FileServer;

internal static class FileRoute
{
    public static void Setup(WebApplication app, ILoggerFactory loggerFactory)
    {
        const string defaultPath = "";
        var logger = loggerFactory.CreateLogger(nameof(FileRoute));

        app.MapGet(
            "{*route}",
            (string? route) =>
            {
                logger.LogInformation("Requested {FileName}...", route);

                if (route is null)
                {
                    return Results.Ok("/");
                }

                var filePath = Path.Combine(defaultPath, route);
                var fileExists = File.Exists(filePath);

                if (fileExists)
                {
                    return Results.Ok(route);
                }
                else
                {
                    return Results.NotFound();
                }
            }
        );
    }
}
