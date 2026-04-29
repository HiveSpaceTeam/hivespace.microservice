using HiveSpace.Core;
using HiveSpace.Core.Extensions;
using HiveSpace.MediaService.Api.Endpoints;
using HiveSpace.MediaService.Core;
using Scalar.AspNetCore;

namespace HiveSpace.MediaService.Api.Extensions;

public static class HostingExtensions
{
    public static WebApplication ConfigureServices(this WebApplicationBuilder builder)
    {
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
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.MapScalarApiReference(options => options
                .WithTitle("HiveSpace MediaService API")
                .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient));
        }

        app.UseHttpsRedirection();

        app.UseHiveSpaceExceptionHandler();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapMediaEndpoints();

        return app;
    }
}
