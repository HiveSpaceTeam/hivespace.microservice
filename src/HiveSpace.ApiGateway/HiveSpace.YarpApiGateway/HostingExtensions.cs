using HiveSpace.YarpApiGateway.Middleware;
using Serilog;

namespace HiveSpace.YarpApiGateway.Extensions;

internal static class HostingExtensions
{
    public static async Task<WebApplication> ConfigureServicesAsync(this WebApplicationBuilder builder)
    {
        builder.AddDefaultSerilog();
        builder.AddServiceDefaults();
        builder.Services.AddAppReverseProxy(builder.Configuration);
        builder.Services.AddAppCors(builder.Configuration);
        await builder.Services.AddAppGatewayTokenValidationAsync(builder.Configuration);
        builder.Services.AddAppHealthChecks();

        return builder.Build();
    }

    public static WebApplication ConfigurePipeline(this WebApplication app)
    {
        app.UseSerilogRequestLogging();
        app.UseCors();
        app.UseWebSockets();
        app.UseMiddleware<CsrfValidationMiddleware>();
        app.UseMiddleware<SessionForwardingMiddleware>();

        app.MapDefaultEndpoints();
        app.MapReverseProxy();

        return app;
    }
}
