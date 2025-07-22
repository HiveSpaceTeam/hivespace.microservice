using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using HiveSpace.Domain.Shared;

namespace HiveSpace.Infrastructure.Persistence.Outbox;

/// <summary>
/// Background service that processes messages from the outbox table,
/// publishing them to the message broker.
/// </summary>
public class OutboxMessageProcessor : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OutboxMessageProcessor> _logger;
    private readonly TimeSpan _pollingInterval = TimeSpan.FromSeconds(5); // Configure this

    public OutboxMessageProcessor(IServiceProvider serviceProvider, ILogger<OutboxMessageProcessor> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Outbox Message Processor started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            await ProcessOutboxMessagesAsync(stoppingToken);
            await Task.Delay(_pollingInterval, stoppingToken);
        }

        _logger.LogInformation("Outbox Message Processor stopped.");
    }

    private async Task ProcessOutboxMessagesAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DbContext>();

        var messagesToProcess = await dbContext.Set<OutboxMessage>()
            .Where(m => m.ProcessedOnUtc == null)
            .OrderBy(m => m.OccurredOnUtc)
            .Take(100) // Process in batches
            .ToListAsync(stoppingToken);

        foreach (var message in messagesToProcess)
        {
            try
            {
                // Deserialize the event
                var eventType = Type.GetType(message.Type);
                if (eventType == null)
                {
                    _logger.LogError("Could not load type {EventType} for outbox message {MessageId}", message.Type, message.Id);
                    message.Error = $"Could not load type {message.Type}";
                    message.Attempts++;
                    continue;
                }

                var domainEvent = JsonSerializer.Deserialize(message.Content, eventType);
                if (domainEvent == null)
                {
                    _logger.LogError("Could not deserialize content for outbox message {MessageId}", message.Id);
                    message.Error = "Could not deserialize content";
                    message.Attempts++;
                    continue;
                }

                // TODO: Publish the message to the message broker
                // This would typically use your messaging infrastructure
                // await messagePublisher.PublishAsync(domainEvent, stoppingToken);

                message.ProcessedOnUtc = DateTime.UtcNow;
                message.Attempts++;
                _logger.LogInformation("Outbox message {MessageId} of type {EventType} published successfully.", message.Id, message.Type);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish outbox message {MessageId} of type {EventType}.", message.Id, message.Type);
                message.Error = ex.Message;
                message.Attempts++;
            }
            finally
            {
                // Save changes to mark message as processed or update attempts/error
                await dbContext.SaveChangesAsync(stoppingToken);
            }
        }
    }
} 