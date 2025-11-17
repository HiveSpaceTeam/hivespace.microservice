using MassTransit;
using Microsoft.Extensions.Logging;

namespace HiveSpace.Infrastructure.Messaging.Observers;

public class PublishObserver : IPublishObserver
{
    private readonly ILogger<PublishObserver> _logger;

    public PublishObserver(ILogger<PublishObserver> logger)
    {
        _logger = logger;
    }

    public Task PostPublish<T>(PublishContext<T> context) where T : class
    {
        _logger.LogInformation("Published {MessageType} with CorrelationId {CorrelationId}", typeof(T).Name, context.CorrelationId ?? context.MessageId);
        return Task.CompletedTask;
    }

    public Task PrePublish<T>(PublishContext<T> context) where T : class
    {
        _logger.LogDebug("Publishing {MessageType}", typeof(T).Name);
        return Task.CompletedTask;
    }

    public Task PublishFault<T>(PublishContext<T> context, Exception exception) where T : class
    {
        _logger.LogError(exception, "Failed to publish {MessageType}", typeof(T).Name);
        return Task.CompletedTask;
    }
}

