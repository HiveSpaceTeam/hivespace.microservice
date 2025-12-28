using HiveSpace.MediaService.Core.Data;
using HiveSpace.MediaService.Core.Interfaces;
using HiveSpace.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using HiveSpace.MediaService.Core.Services;


namespace HiveSpace.MediaService.Api.Extensions
{
    internal static class ServiceCollectionExtensions
    {
        public static void AddAppApplicationServices(this IServiceCollection services)
        {
            // Register application services here
            services.AddScoped<IStorageService, AzureBlobStorageService>();
            services.AddScoped<IQueueService, AzureQueueService>();
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ServiceCollectionExtensions).Assembly));
        }

        public static IServiceCollection AddAppPersistence(this IServiceCollection services, IConfiguration configuration)
        {
            // Add Persistence
            var connectionString = configuration["Database:MediaServiceDb"] 
                ?? throw new InvalidOperationException("Connection string 'Database:MediaServiceDb' not found.");

            services.AddDbContext<MediaDbContext>((sp, options) =>
            {
                options.UseSqlServer(connectionString);
            });

            services.AddPersistenceInfrastructure<MediaDbContext>();

            return services;
        }
    }
}
