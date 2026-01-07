using HiveSpace.CatalogService.Application;
using HiveSpace.CatalogService.Application.Commands;
using HiveSpace.CatalogService.Application.Interfaces;
using HiveSpace.CatalogService.Application.Queries;
using HiveSpace.CatalogService.Application.Services;
using HiveSpace.Core.Filters;

namespace HiveSpace.CatalogService.API.Extentions
{
    internal static class ServiceCollectionExtensions
    {
        public static void AddAppApiControllers(this IServiceCollection services)
        {
            services.AddControllers(options =>
            {
                options.Filters.Add<CustomExceptionFilter>();
            });
        }
        public static void AddAppApplicationServices(this IServiceCollection services)
        {
            // Add MediatR and register handlers
            services.AddApplication();
            services.AddScoped<ICategoryService, CategoryService>();
        }
    }
}
