using HiveSpace.MediaService.Api.Endpoints;

namespace HiveSpace.MediaService.Api.Extensions;

public static class HostingExtensions
{
    public static WebApplication ConfigurePipeline(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapMediaEndpoints();

        return app;
    }
}
