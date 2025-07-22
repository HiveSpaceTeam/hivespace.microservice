using Serilog;
using Microsoft.EntityFrameworkCore;
using HiveSpace.IdentityService.Infrastructure.Data;
using HiveSpace.Infrastructure.Persistence;
using HiveSpace.Core;

namespace HiveSpace.IdentityService.Application.Extensions;

internal static class HostingExtensions
{
    public const string IdentityServiceDbConnection = "IdentityServiceDb";

    public static WebApplication ConfigureServices(this WebApplicationBuilder builder, IConfiguration configuration)
    {
        builder.Services.AddRazorPages();
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();

        builder.Services.AddAppDbContext(configuration);
        builder.Services.AddScoped<DbContext, IdentityDbContext>();
        builder.Services.AddCoreServices();
        builder.Services.AddPersistenceInfrastructure();
        builder.Services.AddAppIdentity();
        builder.Services.AddFluentValidationServices();
        builder.Services.AddAppInfrastructure();
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
