using MassTransit;
using Microsoft.Extensions.Logging;

namespace HiveSpace.Infrastructure.Messaging.Observers;

public class SendObserver : ISendObserver
{
    private readonly ILogger<SendObserver> _logger;

    public SendObserver(ILogger<SendObserver> logger)
    {
        _logger = logger;
    }

    public Task PostSend<T>(SendContext<T> context) where T : class
    {
        _logger.LogInformation("Sent {MessageType} to {DestinationAddress}", typeof(T).Name, context.DestinationAddress);
        return Task.CompletedTask;
    }

    public Task PreSend<T>(SendContext<T> context) where T : class
    {
        _logger.LogDebug("Sending {MessageType} to {DestinationAddress}", typeof(T).Name, context.DestinationAddress);
        return Task.CompletedTask;
    }

    public Task SendFault<T>(SendContext<T> context, Exception exception) where T : class
    {
        _logger.LogError(exception, "Failed to send {MessageType} to {DestinationAddress}", typeof(T).Name, context.DestinationAddress);
        return Task.CompletedTask;
    }
}

