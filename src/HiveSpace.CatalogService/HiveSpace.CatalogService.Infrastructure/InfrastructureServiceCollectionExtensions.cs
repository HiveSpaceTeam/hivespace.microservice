using HiveSpace.CatalogService.Infrastructure.Data;
using HiveSpace.CatalogService.Infrastructure.Repositories;
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

            services.AddScoped<ProductRepository>();
            services.AddScoped<CategoryRepository>();
            services.AddScoped<AttributeRepository>();
            // Add more repositories as needed

            return services;
        }
    }
}
