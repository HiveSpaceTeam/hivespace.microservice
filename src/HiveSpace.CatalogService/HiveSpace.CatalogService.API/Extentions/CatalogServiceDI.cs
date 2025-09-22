using HiveSpace.CatalogService.Application.Interfaces;
using HiveSpace.CatalogService.Application.Services;

namespace HiveSpace.CatalogService.API.Extentions
{
    public static class CatalogServiceDI
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.ConfigureApplicationService();
            return services;
        }
        private static IServiceCollection ConfigureApplicationService(this IServiceCollection services)
        {
            services.AddScoped<IProductService, ProductService>();
            return services;
        }

        //public static IServiceCollection ConfigureCors(this IServiceCollection services, IConfiguration configuration)
        //{
        //    var appConfig = configuration.GetSection("AppConfig").Get<AppConfig>();

        //    if (appConfig?.Cors != null && appConfig.Cors.Length > 0)
        //    {
        //        services.AddCors(options =>
        //        {
        //            options.AddPolicy("_myAllowSpecificOrigins", builder =>
        //            {
        //                builder.WithOrigins(appConfig.Cors)
        //                       .AllowAnyHeader()
        //                       .AllowAnyMethod()
        //                       .AllowCredentials();
        //            });
        //        });
        //    }
        //    return services;
        //}

    }
}
