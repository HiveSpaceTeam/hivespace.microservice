using HiveSpace.Infrastructure.Messaging.Abstractions;

namespace HiveSpace.Infrastructure.Persistence.Transaction;

/// <summary>
/// Adapter that bridges messaging transaction behavior with EF Core transactions.
/// </summary>
public class TransactionalExecutionScope : ITransactionalExecutionScope
{
    private readonly ITransactionService _transactionService;

    public TransactionalExecutionScope(ITransactionService transactionService)
    {
        _transactionService = transactionService;
    }

    public Task ExecuteAsync(string actionName, Func<CancellationToken, Task> action, bool ensureIdempotence = true, CancellationToken cancellationToken = default)
    {
        return _transactionService.InTransactionScopeAsync(_ => action(cancellationToken), ensureIdempotence, actionName);
    }

    public async Task<TResult> ExecuteAsync<TResult>(string actionName, Func<CancellationToken, Task<TResult>> action, bool ensureIdempotence = true, CancellationToken cancellationToken = default)
    {
        TResult result = default!;
        await _transactionService.InTransactionScopeAsync(async _ =>
        {
            result = await action(cancellationToken);
        }, ensureIdempotence, actionName);
        return result;
    }
}

