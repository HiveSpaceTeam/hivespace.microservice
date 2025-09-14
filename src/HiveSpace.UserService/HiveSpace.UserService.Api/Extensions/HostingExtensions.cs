using HiveSpace.Core;
// using HiveSpace.UserService.Application.Extensions;
using HiveSpace.UserService.Infrastructure;
using Serilog;

namespace HiveSpace.UserService.Api.Extensions;

internal static class HostingExtensions
{

    public static WebApplication ConfigureServices(this WebApplicationBuilder builder, IConfiguration configuration)
    {
        // builder.Services.AddMediatRRegistration(configuration);
        builder.Services.AddAppApiControllers();
        builder.Services.AddRazorPages();
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();

        builder.Services.AddAppInfrastructure();
        builder.Services.AddUserDbContext(configuration);
        builder.Services.AddCoreServices();
        builder.Services.AddAppDomainServices();
        builder.Services.AddAppIdentity();
        builder.Services.AddAppApplicationServices();
        builder.Services.AddAppIdentityServer(configuration);
        builder.Services.AddAppAuthentication(configuration);
        builder.Services.AddAppAuthorization();
        builder.Services.AddAppApiVersioning();

        return builder.Build();
    }

    public static WebApplication ConfigurePipeline(this WebApplication app)
    {
        app.UseSerilogRequestLogging();
        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseStaticFiles();
        app.UseRouting();

        app.UseIdentityServer();
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();
        app.MapRazorPages().RequireAuthorization();

        // Uncomment to enable health checks
        // app.UseHealthChecks("/health",
        //     new HealthCheckOptions
        //     {
        //         ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        //     }
        // );

        return app;
    }
}
