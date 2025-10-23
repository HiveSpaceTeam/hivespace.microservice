using HiveSpace.CatalogService.Infrastructure;
using HiveSpace.Core;

namespace HiveSpace.CatalogService.API.Extentions
{
    internal static class HostingExtensions
    {
        public static WebApplication ConfigureServices(this WebApplicationBuilder builder, IConfiguration configuration)
        {
            builder.Services.AddAppApiControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddAppApplicationServices();
            builder.Services.AddCatalogDbContext(configuration);

            return builder.Build();
        }

        public static WebApplication ConfigurePipeline(this WebApplication app)
        {
            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();

            return app;
        }
    }
}

