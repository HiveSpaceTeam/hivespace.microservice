using HiveSpace.CatalogService.Application;
using HiveSpace.CatalogService.Application.CatalogImports.Queueing;
using HiveSpace.CatalogService.Infrastructure;
using HiveSpace.CatalogService.Infrastructure.Data;
using HiveSpace.Core.Contexts;
using HiveSpace.Core.Functions.Queueing;
using HiveSpace.Infrastructure.Messaging.Extensions;
using HiveSpace.Infrastructure.Persistence;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

var builder = FunctionsApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Configuration.AddJsonFile("local.settings.json", optional: true, reloadOnChange: true);

var configuration = builder.Configuration;

builder.Services.AddFunctionQueueMode(configuration);
builder.Services.AddScoped<IRequestContext, RequestContext>();
builder.Services.AddApplication();
builder.Services.AddCatalogDbContext(configuration);
builder.Services.AddPersistenceInfrastructure<CatalogDbContext>();
builder.Services.AddScoped<CatalogImportFunctionDispatcher>();

var queueMode = FunctionQueueModeOptions
    .FromConfiguration(configuration)
    .GetRequiredMode();

if (queueMode.IsRabbitMqMode())
{
    await EnsureRabbitMqQueueAsync(configuration);
    builder.Services.AddMassTransitWithRabbitMq<CatalogDbContext>(configuration, "catalog-func");
}
else if (queueMode.IsAzureServiceBusMode())
{
    builder.Services.AddMassTransitWithAzureServiceBus<CatalogDbContext>(configuration, "catalog-func");
}

builder.Build().Run();

static async Task EnsureRabbitMqQueueAsync(IConfiguration configuration)
{
    var options = FunctionQueueModeOptions.FromConfiguration(configuration);
    var connectionString = options.RabbitMQ.ConnectionString
        ?? throw new InvalidOperationException("RabbitMQ connection string is required.");
    var factory = new ConnectionFactory
    {
        Uri = new Uri(connectionString),
        ClientProvidedName = "hivespace-catalog-import-function-startup"
    };

    await using var connection = await factory.CreateConnectionAsync();
    await using var channel = await connection.CreateChannelAsync();

    await channel.QueueDeclareAsync(
        queue: CatalogImportQueueConstants.QueueName,
        durable: true,
        exclusive: false,
        autoDelete: false,
        arguments: null);
}
