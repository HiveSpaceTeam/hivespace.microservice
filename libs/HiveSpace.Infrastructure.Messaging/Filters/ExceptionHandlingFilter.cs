using MassTransit;
using Microsoft.Extensions.Logging;

namespace HiveSpace.Infrastructure.Messaging.Filters;

/// <summary>
/// Captures unhandled exceptions and writes them to the structured logs.
/// </summary>
public class ExceptionHandlingFilter<T> : IFilter<ConsumeContext<T>> where T : class
{
    private readonly ILogger<ExceptionHandlingFilter<T>> _logger;

    public ExceptionHandlingFilter(ILogger<ExceptionHandlingFilter<T>> logger)
    {
        _logger = logger;
    }

    public async Task Send(ConsumeContext<T> context, IPipe<ConsumeContext<T>> next)
    {
        try
        {
            await next.Send(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception while consuming {MessageType}. CorrelationId: {CorrelationId}", typeof(T).Name, context.CorrelationId);
            throw;
        }
    }

    public void Probe(ProbeContext context)
    {
        context.CreateScope("exceptionHandlingFilter");
    }
}

