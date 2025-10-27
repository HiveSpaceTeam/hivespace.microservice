using HiveSpace.CatalogService.Infrastructure.Data;
using HiveSpace.CatalogService.Infrastructure.Repositories;
using HiveSpace.CatalogService.Infrastructure.Queries;
using HiveSpace.CatalogService.Application.Queries;
using HiveSpace.CatalogService.Application.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using HiveSpace.Core.Exceptions;
using HiveSpace.Core.Exceptions.Models;

namespace HiveSpace.CatalogService.Infrastructure
{
    public static class InfrastructureServiceCollectionExtensions
    {
        public static IServiceCollection AddCatalogDbContext(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("CatalogDb");
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                var error = new Error(CommonErrorCode.ConfigurationMissing, "CatalogDb");
                throw new ConfigurationException(new[] { error });
            }

            services.AddDbContext<CatalogDbContext>(options =>
                options.UseSqlServer(connectionString));

            // Register CatalogService repositories
            services.AddCatalogServiceRepositories();
            
            return services;
        }

        public static void AddCatalogServiceRepositories(this IServiceCollection services)
        {
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<IAttributeRepository, AttributeRepository>();
            services.AddScoped<ICategoryDataQuery, CategoryDataQuery>();
        }

    }
}
