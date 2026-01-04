using HiveSpace.CatalogService.Application.Queries;
using HiveSpace.CatalogService.Domain.Repositories.Domain;
using HiveSpace.CatalogService.Domain.Repositories.External;
using HiveSpace.CatalogService.Infrastructure.Data;
using HiveSpace.CatalogService.Infrastructure.Queries;
using HiveSpace.CatalogService.Infrastructure.Repositories.Domain;
using HiveSpace.Core.Exceptions;
using HiveSpace.Core.Exceptions.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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


            services.AddScoped<IStoreSnapshotRepository, StoreSnapshotRepository>();
        }

    }
}
