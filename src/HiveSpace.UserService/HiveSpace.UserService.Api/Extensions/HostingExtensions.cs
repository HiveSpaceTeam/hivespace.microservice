using HiveSpace.Core;
// using HiveSpace.UserService.Application.Extensions;
using HiveSpace.UserService.Infrastructure;
using Serilog;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace HiveSpace.UserService.Api.Extensions;

internal static class HostingExtensions
{

    public static WebApplication ConfigureServices(this WebApplicationBuilder builder, IConfiguration configuration)
    {
        // builder.Services.AddMediatRRegistration(configuration);
        builder.Services.AddAppApiControllers();
        builder.Services.AddRazorPages();
        builder.Services.AddAppSwagger();
        
        // Add Session support
        builder.Services.AddDistributedMemoryCache();
        builder.Services.AddSession(options =>
        {
            options.IdleTimeout = TimeSpan.FromMinutes(30);
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
            options.Cookie.SameSite = SameSiteMode.Lax;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        });
        
        builder.Services.AddUserDbContext(configuration);
        builder.Services.AddCoreServices();
        builder.Services.AddAppDomainServices();
        builder.Services.AddLocalizationServices(); // Add localization support
        builder.Services.AddAppIdentity();
        builder.Services.AddAppApplicationServices();
        builder.Services.AddEmailConfig(configuration);
        builder.Services.AddAppIdentityServer(configuration);
        builder.Services.AddAppAuthentication(configuration);
        builder.Services.AddAppAuthorization();
        builder.Services.AddAppApiVersioning();

        return builder.Build();
    }

    public static WebApplication ConfigurePipeline(this WebApplication app)
    {
        app.UseSerilogRequestLogging();
        
        // CRITICAL: Use forwarded headers FIRST - before any other middleware
        // This ensures Azure Container Apps HTTPS termination is properly handled
        app.UseForwardedHeaders();
        
        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            
            // Enable Swagger in development
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "HiveSpace User Service API v1");
                options.RoutePrefix = "swagger";
                options.DocumentTitle = "HiveSpace User Service API";
                options.DocExpansion(DocExpansion.List);
                options.DefaultModelsExpandDepth(-1);
                options.DisplayRequestDuration();
            });
        }

        app.UseStaticFiles();
        app.UseRouting();

        // Add session middleware before culture middleware
        app.UseSession();

        // Add culture middleware before authentication to ensure language is set correctly
        app.UseMiddleware<HiveSpace.UserService.Api.Middleware.CultureMiddleware>();

        app.UseIdentityServer();
        app.UseAuthentication();
        app.UseAuthorization();

        // Map all controllers under /user prefix using built-in .NET support
        app.MapControllers();
        
        app.MapRazorPages().RequireAuthorization();

        // Redirect root URL to Swagger in development
        if (app.Environment.IsDevelopment())
        {
            app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();
        }

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
