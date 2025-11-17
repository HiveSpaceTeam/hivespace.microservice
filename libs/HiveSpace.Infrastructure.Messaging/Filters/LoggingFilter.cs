using MassTransit;
using Microsoft.Extensions.Logging;

namespace HiveSpace.Infrastructure.Messaging.Filters;

/// <summary>
/// Logs consume boundaries for easier troubleshooting.
/// </summary>
public class LoggingFilter<T> : IFilter<ConsumeContext<T>> where T : class
{
    private readonly ILogger<LoggingFilter<T>> _logger;

    public LoggingFilter(ILogger<LoggingFilter<T>> logger)
    {
        _logger = logger;
    }

    public async Task Send(ConsumeContext<T> context, IPipe<ConsumeContext<T>> next)
    {
        _logger.LogInformation("Consuming message {MessageType} with CorrelationId {CorrelationId}", typeof(T).Name, context.CorrelationId ?? Guid.Empty);
        await next.Send(context);
        _logger.LogInformation("Completed message {MessageType}", typeof(T).Name);
    }

    public void Probe(ProbeContext context)
    {
        context.CreateScope("loggingFilter");
    }
}

