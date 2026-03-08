using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using HiveSpace.MediaService.Core.Infrastructure.Data;
using HiveSpace.MediaService.Core.Infrastructure.Storage;
using HiveSpace.MediaService.Core.Interfaces;
using HiveSpace.MediaService.Core.Services;
using HiveSpace.MediaService.Core.Configuration;
using FluentValidation;
using HiveSpace.MediaService.Core.Validators;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureAppConfiguration((context, config) =>
    {
        config.AddJsonFile("local.settings.json", optional: true, reloadOnChange: true);
    })
    .ConfigureServices((context, services) =>
    {
        services.Configure<System.Text.Json.JsonSerializerOptions>(options =>
        {
            options.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        });

        var configuration = context.Configuration;
        var baseConnectionString = configuration["Database:MediaServiceDb"];

        // Add connection timeout to handle Azure SQL cold starts
        var connectionStringBuilder = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(baseConnectionString)
        {
            ConnectTimeout = 60,
            ConnectRetryCount = 3,
            ConnectRetryInterval = 10,
            Pooling = true,
            MinPoolSize = 0,
            MaxPoolSize = 100
        };
        var connectionString = connectionStringBuilder.ConnectionString;

        // Register Configuration
        services.AddSingleton<StorageConfiguration>();

        // Register Core Services
        services.AddScoped<IStorageService, AzureBlobStorageService>();
        services.AddScoped<IQueueService, AzureQueueService>();
        services.AddScoped<IMediaService, MediaService>();
        services.AddScoped<IMediaCleanupService, MediaCleanupService>();

        // Register Validators (used by queue/timer functions if needed)
        services.AddValidatorsFromAssemblyContaining<PresignUrlRequestValidator>();

        // Register Database with enhanced retry logic for Azure SQL
        services.AddDbContext<MediaDbContext>((sp, options) =>
        {
            options.UseSqlServer(connectionString, sqlOptions => sqlOptions
                .EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorNumbersToAdd: null)
                .CommandTimeout(120));
        });
    })
    .Build();

host.Run();
