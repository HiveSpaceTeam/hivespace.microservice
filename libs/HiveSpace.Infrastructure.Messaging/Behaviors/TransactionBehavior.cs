using MediatR;
using Microsoft.Extensions.Logging;

namespace HiveSpace.Infrastructure.Messaging.Behaviors;

/// <summary>
/// Simple MediatR pipeline behavior that just calls the next handler.
/// </summary>
public class TransactionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<TransactionBehavior<TRequest, TResponse>> _logger;

    public TransactionBehavior(ILogger<TransactionBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;

        _logger.LogDebug("Executing handler for {RequestName}", requestName);
        return next();
    }
}

