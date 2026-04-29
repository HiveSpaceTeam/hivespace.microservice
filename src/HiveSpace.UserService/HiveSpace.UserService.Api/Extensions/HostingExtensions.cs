using HiveSpace.Core;
using HiveSpace.Infrastructure.Messaging.Extensions;
using HiveSpace.UserService.Infrastructure;
using HiveSpace.UserService.Infrastructure.Data;
using HiveSpace.Infrastructure.Messaging.Configurations;
using Scalar.AspNetCore;
using Serilog;

namespace HiveSpace.UserService.Api.Extensions;

internal static class HostingExtensions
{

    public static WebApplication ConfigureServices(this WebApplicationBuilder builder, IConfiguration configuration)
    {
        // builder.Services.AddMediatRRegistration(configuration);
        builder.Services.AddAppApiControllers();
        builder.Services.AddRazorPages();
        builder.Services.AddAppOpenApi();
        
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

        var messagingOptions = configuration.GetSection(MessagingOptions.SectionName).Get<MessagingOptions>();

        if (messagingOptions?.EnableRabbitMq == true)
        {
            builder.Services.AddMassTransitWithRabbitMq<UserDbContext>(configuration, cfg =>
            {
            });
        }

        return builder.Build();
    }

    public static WebApplication ConfigurePipeline(this WebApplication app)
    {
        app.UseSerilogRequestLogging();
        app.UseForwardedHeaders();
        
        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            
            app.UseSwagger();
            app.MapScalarApiReference(options => options
                .WithTitle("HiveSpace UserService API")
                .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient));
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

        if (app.Environment.IsDevelopment())
        {
            app.MapGet("/", () => Results.Redirect("/scalar/v1")).ExcludeFromDescription();
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
