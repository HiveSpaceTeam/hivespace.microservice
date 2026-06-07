using HiveSpace.Core.Extensions;
using HiveSpace.IdentityService.Api.Endpoints;
using HiveSpace.IdentityService.Api.Middleware;
using Microsoft.Extensions.Hosting;
using Scalar.AspNetCore;
using Serilog;

namespace HiveSpace.IdentityService.Api.Extensions;

internal static class HostingExtensions
{
    public static WebApplication ConfigureServices(this WebApplicationBuilder builder)
    {
        var configuration = builder.Configuration;

        builder.AddDefaultSerilog();
        builder.AddServiceDefaults();
        builder.Services.AddAppForwardedHeaders();
        builder.Services.AddAppSessionState();
        builder.Services.AddAppOpenApi();
        builder.Services.AddAppDatabase(configuration);
        builder.Services.AddAppServices();
        builder.Services.AddAppIdentity();
        builder.Services.AddAppIdentityServer(configuration);
        builder.Services.AddAppAuthentication(configuration);
        builder.Services.AddAppMessaging(configuration);
        builder.Services.AddHealthChecks();

        return builder.Build();
    }

    public static WebApplication ConfigurePipeline(this WebApplication app)
    {
        app.UseSerilogRequestLogging();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.MapScalarApiReference(options => options
                .WithTitle("HiveSpace IdentityService API")
                .WithOpenApiRoutePattern("/swagger/{documentName}/swagger.json")
                .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient));
        }

        app.UseForwardedHeaders();
        app.UseHttpsRedirection();

        app.UseRouting();
        app.UseSession();
        app.UseMiddleware<CultureMiddleware>();

        app.UseHiveSpaceExceptionHandler();

        app.UseIdentityServer();
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapIdentityEndpoints();
        app.MapGoogleExternalAuthEndpoints();
        app.MapAdminIdentityEndpoints();
        app.MapDefaultEndpoints();

        return app;
    }
}
