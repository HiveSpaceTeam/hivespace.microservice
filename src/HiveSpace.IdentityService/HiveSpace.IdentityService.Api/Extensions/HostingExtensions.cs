using HiveSpace.Core;
using HiveSpace.IdentityService.Application.Extensions;
using Serilog;

namespace HiveSpace.IdentityService.Api.Extensions;

internal static class HostingExtensions
{
    public const string IdentityServiceDbConnection = "IdentityServiceDb";

    public static WebApplication ConfigureServices(this WebApplicationBuilder builder, IConfiguration configuration)
    {
        builder.Services.AddMediatRRegistration(configuration);
        builder.Services.AddAppApiControllers();
        builder.Services.AddRazorPages();
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();

        builder.Services.AddAppInfrastructure();
        builder.Services.AddAppDbContext(configuration);
        builder.Services.AddCoreServices();
        builder.Services.AddAppIdentity();
        builder.Services.AddFluentValidationServices();
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
        app.UseDeveloperExceptionPage();

        // Uncomment to initialize database on startup
        // DatabaseInitializer.InitializeDatabase(app, app.Configuration);

        app.UseStaticFiles();
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseIdentityServer();

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
