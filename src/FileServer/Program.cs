using Microsoft.AspNetCore.Http.Features;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting.Compact;

namespace FileServer;

internal static class Program
{
    public static async Task Main()
    {
        const long bodyRequestLimit = 10_737_418_240; // 10 GB

        var settings = AppSetting.Load<Settings>();

        using var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;

        var builder = WebApplication.CreateBuilder();
        builder.Logging.ClearProviders();
        builder.Logging.AddSerilog(GetLogger(), true);

        builder.WebHost.ConfigureKestrel(
            options => options.Limits.MaxRequestBodySize = bodyRequestLimit);

        builder.Services.Configure<FormOptions>(
            options => options.MultipartBodyLengthLimit =  bodyRequestLimit);

        var app = builder.Build();

        // This is enabled so we can handle form posts for delete.
        app.UseHttpMethodOverride(new HttpMethodOverrideOptions { FormFieldName = "_method"});
        app.UseRouting();

        var loggerFactory = app.Services.GetService<ILoggerFactory>()
            ?? throw new InvalidOperationException(
                $"{nameof(ILoggerFactory)} is not configured in the IOC container.");

        app.UseMiddleware<BasicAuthMiddleware>(settings.FileServerUsers);

        FileRoute.Setup(app, loggerFactory, settings);

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
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Information()
            .Enrich.FromLogContext()
            .WriteTo.Console(new CompactJsonFormatter())
            .CreateLogger();
    }
}
