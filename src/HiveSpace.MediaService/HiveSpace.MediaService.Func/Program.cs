using HiveSpace.Infrastructure.Messaging.Configurations;
using HiveSpace.MediaService.Core.Infrastructure.Messaging.Publishers;
using HiveSpace.MediaService.Core.Infrastructure.Configuration;
using HiveSpace.MediaService.Core.Persistence;
using HiveSpace.MediaService.Core.Infrastructure.Storage;
using HiveSpace.MediaService.Core.Interfaces;
using HiveSpace.MediaService.Core.Interfaces.Messaging;
using HiveSpace.MediaService.Core.Persistence.Repositories;
using HiveSpace.MediaService.Core.Services;
using MassTransit;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = FunctionsApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Configuration.AddJsonFile("local.settings.json", optional: true, reloadOnChange: true);

builder.Services.Configure<System.Text.Json.JsonSerializerOptions>(options =>
{
    options.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
});

var configuration = builder.Configuration;
var baseConnectionString = configuration.GetConnectionString("MediaDb");

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
builder.Services.AddSingleton<StorageConfiguration>();

// Register Core Services
builder.Services.AddScoped<IStorageService, AzureBlobStorageService>();
builder.Services.AddScoped<IQueueService, AzureQueueService>();
builder.Services.AddScoped<IMediaAssetRepository, MediaAssetRepository>();
builder.Services.AddScoped<IMediaCleanupService, MediaCleanupService>();
builder.Services.AddScoped<IMediaEventPublisher, MediaEventPublisher>();

// Register Database with enhanced retry logic for Azure SQL
builder.Services.AddDbContext<MediaDbContext>((_, options) =>
{
    options.UseSqlServer(connectionString, sqlOptions => sqlOptions
        .EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null)
        .CommandTimeout(120));
});

var messagingOptions = configuration
    .GetSection(MessagingOptions.SectionName)
    .Get<MessagingOptions>();

if (messagingOptions?.EnableRabbitMq == true)
{
    var rabbitMqConnectionString = MessagingConnectionStrings.GetRequired(
        configuration,
        MessagingConnectionStrings.RabbitMq);

    builder.Services.AddMassTransit(x =>
    {
        x.UsingRabbitMq((_, cfg) =>
        {
            cfg.Host(new Uri(rabbitMqConnectionString));
        });
    });
}

builder.Build().Run();
