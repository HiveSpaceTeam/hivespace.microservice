using HiveSpace.UserService.Api.Extensions;
using Microsoft.AspNetCore.HttpOverrides;
using Serilog;
using System.Globalization;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(formatProvider: CultureInfo.InvariantCulture)
    .CreateBootstrapLogger();

Log.Information("Starting up");


try
{
    var builder = WebApplication.CreateBuilder(args);
    var configuration = builder.Configuration;

    // Configure Forwarded Headers for Azure Container Apps
    builder.Services.Configure<ForwardedHeadersOptions>(options =>
    {
        // This tells the app to trust the X-Forwarded-Proto (http/https) header.
        options.ForwardedHeaders =
            ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;

        // This is CRITICAL for Azure Container Apps.
        // It tells the app to trust the proxy even though it's not on a "known network."
        options.KnownNetworks.Clear();
        options.KnownProxies.Clear();
    });

    builder.Host.UseSerilog((ctx, lc) => lc
        .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}", formatProvider: CultureInfo.InvariantCulture)
        .Enrich.FromLogContext()
        .ReadFrom.Configuration(ctx.Configuration));

    var app = builder
        .ConfigureServices(configuration) 
        .ConfigurePipeline();

    Log.Information("Environment: {EnvironmentName}", app.Environment.EnvironmentName);

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Unhandled exception");
}
finally
{
    Log.Information("Shut down complete");
    Log.CloseAndFlush();
}

