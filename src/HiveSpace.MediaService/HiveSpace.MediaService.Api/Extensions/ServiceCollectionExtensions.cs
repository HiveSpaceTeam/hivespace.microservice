using FluentValidation;
using HiveSpace.MediaService.Core.Configuration;
using HiveSpace.MediaService.Core.Infrastructure.Data;
using HiveSpace.MediaService.Core.Infrastructure.Storage;
using HiveSpace.MediaService.Core.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using CoreMediaService = HiveSpace.MediaService.Core.Services.MediaService;
using CoreMediaCleanupService = HiveSpace.MediaService.Core.Services.MediaCleanupService;

namespace HiveSpace.MediaService.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAppServices(this IServiceCollection services)
    {
        services.AddSingleton<StorageConfiguration>();
        services.AddScoped<IStorageService, AzureBlobStorageService>();
        services.AddScoped<IQueueService, AzureQueueService>();
        services.AddScoped<IMediaService, CoreMediaService>();
        services.AddScoped<IMediaCleanupService, CoreMediaCleanupService>();

        return services;
    }

    public static IServiceCollection AddAppValidators(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(StorageConfiguration).Assembly);

        return services;
    }

    public static IServiceCollection AddAppDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        var baseConnectionString = configuration["Database:MediaServiceDb"];

        var connectionStringBuilder = new SqlConnectionStringBuilder(baseConnectionString)
        {
            ConnectTimeout = 60,
            ConnectRetryCount = 3,
            ConnectRetryInterval = 10,
            Pooling = true,
            MinPoolSize = 0,
            MaxPoolSize = 100
        };

        services.AddDbContext<MediaDbContext>((_, options) =>
        {
            options.UseSqlServer(connectionStringBuilder.ConnectionString, sqlOptions => sqlOptions
                .EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorNumbersToAdd: null)
                .CommandTimeout(120));
        });

        return services;
    }
}
