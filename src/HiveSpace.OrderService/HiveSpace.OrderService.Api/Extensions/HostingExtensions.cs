using HiveSpace.Core;
using HiveSpace.Core.Extensions;
using HiveSpace.Core.Middlewares;
using HiveSpace.OrderService.Api.Endpoints;
using HiveSpace.OrderService.Application;
using HiveSpace.OrderService.Infrastructure;
using HiveSpace.OrderService.Infrastructure.Data;
using Microsoft.Extensions.Hosting;
using Scalar.AspNetCore;
using Serilog;

namespace HiveSpace.OrderService.Api.Extensions;

internal static class HostingExtensions
{
    public static WebApplication ConfigureServices(this WebApplicationBuilder builder)
    {
        builder.AddDefaultSerilog();
        builder.AddServiceDefaults();
        builder.Services.AddAppApiControllers();
        builder.Services.AddAppOpenApi();
        builder.Services.AddOrderDbContext(builder.Configuration);
        builder.Services.AddCoreServices();
        builder.Services.AddIdGenerators(builder.Configuration);
        builder.Services.AddAppMessaging(builder.Configuration);
        builder.Services.AddAppAuthentication(builder.Configuration);
        builder.Services.AddApplication();

        return builder.Build();
    }

    public static WebApplication ConfigurePipeline(this WebApplication app)
    {
        app.UseSerilogRequestLogging();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.MapScalarApiReference(options => options
                .WithTitle("HiveSpace OrderService API")
                .WithOpenApiRoutePattern("/swagger/{documentName}/swagger.json")
                .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient));
        }

        app.UseHttpsRedirection();
        app.UseMiddleware<RequestIdMiddleware>();
        app.UseHiveSpaceExceptionHandler();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapDefaultEndpoints();
        app.MapHealthEndpoints();
        app.MapCouponEndpoints();
        app.MapCartEndpoints();
        app.MapCheckoutEndpoints();
        app.MapOrderEndpoints();

        return app;
    }
}
