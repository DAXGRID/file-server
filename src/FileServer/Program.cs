using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting.Compact;

namespace FileServer;

internal static class Program
{
    public static async Task Main()
    {
        using var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;

        var builder = WebApplication.CreateBuilder();
        builder.Logging.ClearProviders();
        builder.Logging.AddSerilog(GetLogger(), true);
        var app = builder.Build();

        var loggerFactory = app.Services.GetService<ILoggerFactory>()
            ?? throw new InvalidOperationException(
                $"{nameof(ILoggerFactory)} is not configured in the IOC container.");

        FileRoute.Setup(app, loggerFactory);

        app.Logger.LogInformation("Starting the web service.");
        var webServerTask = app.RunAsync(cancellationToken).ConfigureAwait(false);
        app.Logger.LogInformation("The web service is now started.");

        await webServerTask;

        app.Logger.LogInformation("Shutting down...");
    }

    private static Logger GetLogger()
    {
        return new LoggerConfiguration()
            .MinimumLevel.Override("System", LogEventLevel.Warning)
            .MinimumLevel.Information()
            .Enrich.FromLogContext()
            .WriteTo.Console(new CompactJsonFormatter())
            .CreateLogger();
    }
}
