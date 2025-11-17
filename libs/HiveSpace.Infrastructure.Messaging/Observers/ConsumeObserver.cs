using MassTransit;
using Microsoft.Extensions.Logging;

namespace HiveSpace.Infrastructure.Messaging.Observers;

public class ConsumeObserver : IConsumeObserver
{
    private readonly ILogger<ConsumeObserver> _logger;

    public ConsumeObserver(ILogger<ConsumeObserver> logger)
    {
        _logger = logger;
    }

    public Task PostConsume<T>(ConsumeContext<T> context) where T : class
    {
        _logger.LogInformation("Consumed {MessageType}", typeof(T).Name);
        return Task.CompletedTask;
    }

    public Task PreConsume<T>(ConsumeContext<T> context) where T : class
    {
        _logger.LogDebug("Starting consume for {MessageType}", typeof(T).Name);
        return Task.CompletedTask;
    }

    public Task ConsumeFault<T>(ConsumeContext<T> context, Exception exception) where T : class
    {
        _logger.LogError(exception, "Consume fault for {MessageType}", typeof(T).Name);
        return Task.CompletedTask;
    }
}

