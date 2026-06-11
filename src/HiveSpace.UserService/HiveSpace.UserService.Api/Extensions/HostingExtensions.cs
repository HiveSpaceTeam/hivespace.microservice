using HiveSpace.Core;
using HiveSpace.Infrastructure.Messaging.Extensions;
using HiveSpace.UserService.Api.Consumers;
using HiveSpace.UserService.Api.Middleware;
using HiveSpace.UserService.Infrastructure;
using HiveSpace.UserService.Infrastructure.Data;
using HiveSpace.Infrastructure.Messaging.Configurations;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Hosting;
using Scalar.AspNetCore;
using Serilog;

namespace HiveSpace.UserService.Api.Extensions;

internal static class HostingExtensions
{

    public static WebApplication ConfigureServices(this WebApplicationBuilder builder, IConfiguration configuration)
    {
        builder.AddDefaultSerilog();
        builder.AddServiceDefaults();
        builder.Services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders =
                ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;

            options.KnownNetworks.Clear();
            options.KnownProxies.Clear();
        });
        // builder.Services.AddMediatRRegistration(configuration);
        builder.Services.AddAppApiControllers();
        builder.Services.AddAppOpenApi();

        builder.Services.AddUserDbContext(configuration);
        builder.Services.AddCoreServices();
        builder.Services.AddAppDomainServices();
        builder.Services.AddLocalizationServices(); // Add localization support
        builder.Services.AddAppApplicationServices();
        builder.Services.AddAppAuthentication(configuration);
        builder.Services.AddAppAuthorization();
        builder.Services.AddAppApiVersioning();

        var messagingOptions = configuration.GetSection(MessagingOptions.SectionName).Get<MessagingOptions>();

        if (messagingOptions?.EnableRabbitMq == true)
        {
            builder.Services.AddMassTransitWithRabbitMq<UserDbContext>(configuration, cfg =>
            {
                cfg.AddConsumer<MediaAssetProcessedConsumer>()
                    .Endpoint(e => e.Name = "user-media-asset-processed");

                cfg.AddConsumer<IdentityUserReadyConsumer>()
                    .Endpoint(e => e.Name = "user-identity-user-ready");
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
                .WithOpenApiRoutePattern("/swagger/{documentName}/swagger.json")
                .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient));
        }

        app.UseRouting();

        // Add culture middleware before authentication to ensure language is set correctly
        app.UseMiddleware<CultureMiddleware>();

        app.UseAuthentication();
        app.UseAuthorization();

        // Map all controllers under /user prefix using built-in .NET support
        app.MapControllers();
        app.MapDefaultEndpoints();
        
        if (app.Environment.IsDevelopment())
        {
            app.MapGet("/", () => Results.Redirect("/scalar/v1")).ExcludeFromDescription();
        }

        return app;
    }
}
