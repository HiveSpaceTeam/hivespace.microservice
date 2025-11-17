using MassTransit;
using Microsoft.Extensions.Logging;

namespace HiveSpace.Infrastructure.Messaging.Filters;

/// <summary>
/// Provides a lightweight retry mechanism for transient failures.
/// </summary>
public class RetryFilter<T> : IFilter<ConsumeContext<T>> where T : class
{
    private readonly ILogger<RetryFilter<T>> _logger;
    private const int DefaultRetryCount = 3;
    private static readonly TimeSpan Delay = TimeSpan.FromMilliseconds(200);

    public RetryFilter(ILogger<RetryFilter<T>> logger)
    {
        _logger = logger;
    }

    public async Task Send(ConsumeContext<T> context, IPipe<ConsumeContext<T>> next)
    {
        var attempt = 0;
        Exception? lastException = null;
        while (attempt < DefaultRetryCount)
        {
            try
            {
                await next.Send(context);
                return;
            }
            catch (Exception ex) when (IsTransient(ex))
            {
                lastException = ex;
                attempt++;
                _logger.LogWarning(ex, "Transient error while consuming {MessageType}. Attempt {Attempt}", typeof(T).Name, attempt);
                await Task.Delay(Delay, context.CancellationToken);
            }
        }

        if (lastException is not null)
        {
            throw lastException;
        }
    }

    public void Probe(ProbeContext context)
    {
        context.CreateScope("retryFilter");
    }

    private static bool IsTransient(Exception ex)
    {
        return ex is TimeoutException || ex is TaskCanceledException;
    }
}

