using HiveSpace.Core;
using HiveSpace.Core.Extensions;
using HiveSpace.NotificationService.Api.Endpoints;
using HiveSpace.NotificationService.Api.Hubs;
using HiveSpace.NotificationService.Core;
using Scalar.AspNetCore;
using Serilog;

namespace HiveSpace.NotificationService.Api.Extensions;

internal static class HostingExtensions
{
    public static WebApplication ConfigureServices(this WebApplicationBuilder builder)
    {
        var configuration = builder.Configuration;

        builder.Services.AddAppOpenApi();
        builder.Services.AddNotificationDbContext(configuration);
        builder.Services.AddCoreServices();
        builder.Services.AddAppSignalR();
        builder.Services.AddAppRedis(configuration);
        builder.Services.AddAppHangfire(configuration);
        builder.Services.AddNotificationCoreServices(configuration);
        builder.Services.AddApplication();
        builder.Services.AddAppMessaging(configuration);
        builder.Services.AddAppAuthentication(configuration);

        return builder.Build();
    }

    public static async Task<WebApplication> ConfigurePipelineAsync(this WebApplication app)
    {
        app.UseSerilogRequestLogging();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.MapScalarApiReference(options => options
                .WithTitle("HiveSpace NotificationService API")
                .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient));

            Console.WriteLine("Attempting to run database migrations...");
            await DataSeeder.EnsureSeedDataAsync(app);
            Console.WriteLine("Database migrations completed.");
        }

        app.UseHiveSpaceExceptionHandler();

        app.UseStaticFiles();
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapNotificationEndpoints();
        app.MapPreferenceEndpoints();

        if (app.Environment.IsDevelopment())
            app.MapDevEndpoints();

        var hubEndpoint = app.MapHub<NotificationHub>("/hubs/notifications");
        if (!app.Environment.IsDevelopment())
            hubEndpoint.RequireAuthorization();

        return app;
    }
}
