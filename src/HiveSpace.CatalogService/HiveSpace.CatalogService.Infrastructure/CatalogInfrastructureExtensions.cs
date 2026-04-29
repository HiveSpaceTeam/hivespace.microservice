using HiveSpace.CatalogService.Application.Categories;
using HiveSpace.CatalogService.Application.Interfaces.Messaging;
using HiveSpace.CatalogService.Application.Products;
using HiveSpace.CatalogService.Domain.Repositories;
using HiveSpace.CatalogService.Domain.Repositories.External;
using HiveSpace.CatalogService.Infrastructure.Data;
using HiveSpace.CatalogService.Infrastructure.DataQueries;
using HiveSpace.CatalogService.Infrastructure.Messaging.Publishers;
using HiveSpace.CatalogService.Infrastructure.Repositories;
using HiveSpace.CatalogService.Infrastructure.Repositories.Externals;
using HiveSpace.CatalogService.Infrastructure.SeedData;
using HiveSpace.Core.Exceptions;
using HiveSpace.Core.Exceptions.Models;
using HiveSpace.Infrastructure.Persistence.Seeding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HiveSpace.CatalogService.Infrastructure
{
    public static class CatalogInfrastructureExtensions
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
                options.UseSqlServer(connectionString, sqlOptions => sqlOptions
                    .EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorNumbersToAdd: null)));

            // Register CatalogService repositories
            services.AddCatalogServiceRepositories();
            
            return services;
        }

        public static void AddCatalogServiceRepositories(this IServiceCollection services)
        {
            services.AddScoped<IProductRepository, SqlProductRepository>();
            services.AddScoped<ICategoryRepository, SqlCategoryRepository>();
            services.AddScoped<IAttributeRepository, SqlAttributeRepository>();
            services.AddScoped<ICategoryDataQuery, CategoryDataQuery>();
            services.AddScoped<IProductDataQuery, ProductDataQuery>();

            services.AddScoped<IProductEventPublisher, ProductEventPublisher>();

            services.AddScoped<IStoreRefRepository, StoreRefRepository>();

            services.AddScoped<ISeeder, CategorySeeder>();
            services.AddScoped<ISeeder, AttributeSeeder>();
            services.AddScoped<ISeeder, StoreSeeder>();
            services.AddScoped<ISeeder, BookstoreSeeder>();
            services.AddScoped<ISeeder, HomeLivingSeeder>();
            services.AddScoped<ISeeder, MobileTabletSeeder>();
        }

    }
}
