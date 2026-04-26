using FluentValidation;
using HiveSpace.Application.Shared.Behaviors;
using HiveSpace.MediaService.Core.Infrastructure.Configuration;
using HiveSpace.MediaService.Core.Infrastructure.Data;
using HiveSpace.MediaService.Core.Infrastructure.Storage;
using HiveSpace.MediaService.Core.Interfaces;
using HiveSpace.MediaService.Core.Features.Media.Commands.GeneratePresignedUrl;
using HiveSpace.MediaService.Core.Persistence.Repositories;
using HiveSpace.MediaService.Core.Services;
using MediatR;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace HiveSpace.MediaService.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAppServices(this IServiceCollection services)
    {
        services.AddSingleton<StorageConfiguration>();
        services.AddScoped<IStorageService, AzureBlobStorageService>();
        services.AddScoped<IQueueService, AzureQueueService>();
        services.AddScoped<IMediaAssetRepository, MediaAssetRepository>();
        services.AddScoped<IMediaCleanupService, MediaCleanupService>();

        return services;
    }

    public static IServiceCollection AddAppMediatR(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(GeneratePresignedUrlCommand).Assembly);
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblyContaining<GeneratePresignedUrlCommand>();
            cfg.AddOpenBehavior(typeof(ValidationPipelineBehavior<,>));
        });
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

    public static IServiceCollection AddAppExceptionHandling(this IServiceCollection services)
    {
        services.AddProblemDetails();
        return services;
    }
}
