using HiveSpace.CatalogService.Infrastructure.Data;
using HiveSpace.CatalogService.Infrastructure.Repositories;
using HiveSpace.CatalogService.Infrastructure.Queries;
using HiveSpace.CatalogService.Application.Queries;
using HiveSpace.CatalogService.Application.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace HiveSpace.CatalogService.Infrastructure
{
    public static class InfrastructureServiceCollectionExtensions
    {
        public static IServiceCollection AddCatalogInfrastructure(this IServiceCollection services, string connectionString)
        {
            services.AddDbContext<CatalogDbContext>(options =>
                options.UseSqlServer(connectionString));

            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<IAttributeRepository, AttributeRepository>();
            services.AddScoped<IQueryService, QueryService>();
            // Add more repositories as needed

            return services;
        }
    }
}
