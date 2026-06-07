using Asp.Versioning;
using HiveSpace.CatalogService.Application;
using HiveSpace.Core;
using Microsoft.Extensions.Hosting;

namespace HiveSpace.CatalogService.Api.Extensions
{
    internal static class ServiceCollectionExtensions
    {
        public static void AddAppApiControllers(this IServiceCollection services)
            => services.AddHiveSpaceControllers();

        public static void AddAppOpenApi(this IServiceCollection services)
            => services.AddDefaultOpenApi("HiveSpace.CatalogService API", "HiveSpace.CatalogService microservice");

        public static void AddAppAuthentication(this IServiceCollection services, IConfiguration configuration)
            => services.AddDefaultAuthentication(configuration, "catalog.fullaccess");

        public static void AddAppApplicationServices(this IServiceCollection services)
            => services.AddApplication();

        public static void AddAppApiVersioning(this IServiceCollection services)
        {
            services.AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ReportApiVersions = true;
            });
        }
    }
}
