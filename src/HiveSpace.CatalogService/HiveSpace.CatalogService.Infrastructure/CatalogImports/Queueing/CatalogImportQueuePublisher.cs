using System.Text;
using Azure.Messaging.ServiceBus;
using HiveSpace.CatalogService.Application.CatalogImports.Queueing;
using HiveSpace.Core.Functions.Queueing;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace HiveSpace.CatalogService.Infrastructure.CatalogImports.Queueing;

public sealed class CatalogImportQueuePublisher(
    FunctionQueueModeOptions queueOptions,
    FunctionQueueMode queueMode,
    ILogger<CatalogImportQueuePublisher> logger)
    : ICatalogImportQueuePublisher
{
    public async Task PublishAsync(
        CatalogImportQueueWorkItem workItem,
        string payload,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "Publishing catalog import job {JobId} attempt {Attempt} via {QueueMode}",
            workItem.JobId,
            workItem.Attempt,
            queueMode);

        if (queueMode.IsRabbitMqMode())
        {
            await SendRabbitMqAsync(payload, workItem, cancellationToken);
            return;
        }

        if (queueMode.IsAzureServiceBusMode())
        {
            await SendServiceBusAsync(payload, workItem, cancellationToken);
            return;
        }

        throw new InvalidOperationException($"Unsupported catalog import queue mode '{queueMode}'.");
    }

    private async Task SendRabbitMqAsync(
        string payload,
        CatalogImportQueueWorkItem workItem,
        CancellationToken cancellationToken)
    {
        var connectionString = queueOptions.RabbitMQ.ConnectionString
            ?? throw new InvalidOperationException("RabbitMQ connection string is required.");
        var factory = new ConnectionFactory
        {
            Uri = new Uri(connectionString),
            ClientProvidedName = "hivespace-catalog-import-outbox"
        };

        await using var connection = await factory.CreateConnectionAsync(cancellationToken);
        await using var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

        await channel.QueueDeclareAsync(
            queue: CatalogImportQueueConstants.QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null,
            cancellationToken: cancellationToken);

        var properties = new BasicProperties
        {
            ContentType = "application/json",
            CorrelationId = workItem.CorrelationId,
            MessageId = CreateMessageId(workItem),
            Persistent = true
        };
        var body = Encoding.UTF8.GetBytes(payload);

        await channel.BasicPublishAsync(
            exchange: string.Empty,
            routingKey: CatalogImportQueueConstants.QueueName,
            mandatory: true,
            basicProperties: properties,
            body: body,
            cancellationToken: cancellationToken);
    }

    private async Task SendServiceBusAsync(
        string payload,
        CatalogImportQueueWorkItem workItem,
        CancellationToken cancellationToken)
    {
        var connectionString = queueOptions.AzureServiceBus.ConnectionString
            ?? throw new InvalidOperationException("Azure Service Bus connection string is required.");

        await using var client = new ServiceBusClient(connectionString);
        var sender = client.CreateSender(CatalogImportQueueConstants.QueueName);
        var message = new ServiceBusMessage(BinaryData.FromString(payload))
        {
            ContentType = "application/json",
            CorrelationId = workItem.CorrelationId,
            MessageId = CreateMessageId(workItem)
        };

        await sender.SendMessageAsync(message, cancellationToken);
    }

    private static string CreateMessageId(CatalogImportQueueWorkItem workItem)
        => $"{workItem.JobId:N}-{workItem.Attempt}";
}
