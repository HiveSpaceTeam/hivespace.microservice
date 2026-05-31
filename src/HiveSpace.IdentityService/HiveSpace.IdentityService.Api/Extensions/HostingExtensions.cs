using HiveSpace.Core.Extensions;
using HiveSpace.IdentityService.Api.Endpoints;
using HiveSpace.IdentityService.Api.Middleware;
using HiveSpace.IdentityService.Core.Infrastructure;
using Scalar.AspNetCore;

namespace HiveSpace.IdentityService.Api.Extensions;

internal static class HostingExtensions
{
    public static WebApplication ConfigureServices(this WebApplicationBuilder builder)
    {
        var configuration = builder.Configuration;

        builder.Services.AddAppRazorUi();
        builder.Services.AddAppOpenApi();
        builder.Services.AddAppDatabase(configuration);
        builder.Services.AddAppServices();
        builder.Services.AddLocalizationServices();
        builder.Services.AddAppIdentity();
        builder.Services.AddAppIdentityServer(configuration);
        builder.Services.AddAppAuthentication(configuration);
        builder.Services.AddAppMessaging(configuration);
        builder.Services.AddHealthChecks();

        return builder.Build();
    }

    public static async Task<WebApplication> ConfigurePipelineAsync(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.MapScalarApiReference(options => options
                .WithTitle("HiveSpace IdentityService API")
                .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient));

            await DataSeeder.EnsureSeedDataAsync(app);
        }

        app.UseHttpsRedirection();

        app.UseStaticFiles();
        app.UseRouting();
        app.UseSession();
        app.UseMiddleware<CultureMiddleware>();

        app.UseHiveSpaceExceptionHandler();

        app.UseIdentityServer();
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapAccountCompatibilityEndpoints();
        app.MapIdentityEndpoints();
        app.MapAdminIdentityEndpoints();
        app.MapHealthChecks("/health");
        app.MapRazorPages().RequireAuthorization();

        return app;
    }
}
