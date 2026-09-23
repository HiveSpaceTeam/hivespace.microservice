using HiveSpace.Core.Functions.Queueing;
using HiveSpace.Infrastructure.Messaging.Extensions;
using HiveSpace.MediaService.Api.Infrastructure.Messaging;
using HiveSpace.MediaService.Core.Infrastructure.Configuration;
using HiveSpace.MediaService.Core.Infrastructure.Messaging.Publishers;
using HiveSpace.MediaService.Core.Infrastructure.Storage;
using HiveSpace.MediaService.Core.Interfaces;
using HiveSpace.MediaService.Core.Interfaces.Messaging;
using HiveSpace.MediaService.Core.Persistence;
using HiveSpace.MediaService.Core.Persistence.Repositories;
using HiveSpace.MediaService.Core.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

namespace HiveSpace.MediaService.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAppServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<StorageConfiguration>();
        services.AddScoped<IStorageService, AzureBlobStorageService>();
        services.AddFunctionQueueMode(configuration);
        services.AddScoped<IQueueService, MediaProcessingQueueService>();
        services.AddScoped<IMediaAssetRepository, MediaAssetRepository>();
        services.AddScoped<IMediaCleanupService, MediaCleanupService>();
        services.AddScoped<IMediaEventPublisher, MediaEventPublisher>();

        var queueMode = FunctionQueueModeOptions
            .FromConfiguration(configuration)
            .GetRequiredMode();

        if (queueMode.IsRabbitMqMode())
        {
            services.AddMassTransitWithRabbitMq<MediaDbContext>(
                configuration,
                "media-api");
        }
        else if (queueMode.IsAzureServiceBusMode())
        {
            services.AddMassTransitWithAzureServiceBus<MediaDbContext>(
                configuration,
                "media-api");
        }

        return services;
    }

    public static IServiceCollection AddAppDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        var baseConnectionString = configuration.GetConnectionString("MediaDb");

        services.AddDbContext<MediaDbContext>((_, options) =>
        {
            options.UseSqlServer(baseConnectionString, sqlOptions => sqlOptions
                .EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorNumbersToAdd: null)
                .CommandTimeout(120));
        });

        return services;
    }

    public static IServiceCollection AddAppOpenApi(this IServiceCollection services)
        => services.AddDefaultOpenApi("HiveSpace.MediaService API", "HiveSpace.MediaService microservice");

    public static IServiceCollection AddAppAuthentication(this IServiceCollection services, IConfiguration configuration)
        => services.AddDefaultAuthentication(configuration, "media.fullaccess");
}
