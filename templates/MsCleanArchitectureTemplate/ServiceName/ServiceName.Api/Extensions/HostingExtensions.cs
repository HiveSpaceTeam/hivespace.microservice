namespace ServiceName.Api.Extensions;

internal static class HostingExtensions
{

    public static WebApplication ConfigureServices(this WebApplicationBuilder builder, IConfiguration configuration)
    {
        return builder.Build();
    }

    public static WebApplication ConfigurePipeline(this WebApplication app)
    {
        return app;
    }
}
