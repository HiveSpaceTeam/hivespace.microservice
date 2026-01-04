using HiveSpace.CatalogService.Application.Commands;
using HiveSpace.CatalogService.Application.Interfaces;
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
            services.AddScoped<ICategoryService, CategoryService>();

            services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<CreateProductCommand>());

        }
    }
}
