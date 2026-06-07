using HiveSpace.Core;
using HiveSpace.Core.Extensions;
using HiveSpace.MediaService.Api.Endpoints;
using HiveSpace.MediaService.Core;
using Microsoft.Extensions.Hosting;
using Scalar.AspNetCore;
using Serilog;

namespace HiveSpace.MediaService.Api.Extensions;

public static class HostingExtensions
{
    public static WebApplication ConfigureServices(this WebApplicationBuilder builder)
    {
        builder.AddDefaultSerilog();
        builder.AddServiceDefaults();
        builder.Services.AddAppOpenApi();
        builder.Services.AddAppDatabase(builder.Configuration);
        builder.Services.AddCoreServices();
        builder.Services.AddAppServices();
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
                .WithTitle("HiveSpace MediaService API")
                .WithOpenApiRoutePattern("/swagger/{documentName}/swagger.json")
                .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient));
        }

        app.UseHttpsRedirection();

        app.UseHiveSpaceExceptionHandler();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapDefaultEndpoints();
        app.MapMediaEndpoints();

        return app;
    }
}
