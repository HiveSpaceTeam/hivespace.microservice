using HiveSpace.Infrastructure.Messaging.Abstractions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HiveSpace.Infrastructure.Messaging.Behaviors;

/// <summary>
/// Wraps MediatR handlers inside a transactional boundary.
/// </summary>
public class TransactionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ITransactionalExecutionScope _transactionScope;
    private readonly ILogger<TransactionBehavior<TRequest, TResponse>> _logger;

    public TransactionBehavior(ITransactionalExecutionScope transactionScope, ILogger<TransactionBehavior<TRequest, TResponse>> logger)
    {
        _transactionScope = transactionScope;
        _logger = logger;
    }

    public Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;

        return _transactionScope.ExecuteAsync(requestName, async _ =>
        {
            _logger.LogDebug("Executing transactional handler for {RequestName}", requestName);
            return await next();
        }, ensureIdempotence: true, cancellationToken);
    }
}

