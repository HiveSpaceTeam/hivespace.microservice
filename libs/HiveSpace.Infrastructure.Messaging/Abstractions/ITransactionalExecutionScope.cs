namespace HiveSpace.Infrastructure.Messaging.Abstractions;

/// <summary>
/// Contract used by the application pipeline to wrap handlers inside a transactional boundary.
/// </summary>
public interface ITransactionalExecutionScope
{
    Task ExecuteAsync(string actionName, Func<CancellationToken, Task> action, bool ensureIdempotence = true, CancellationToken cancellationToken = default);

    Task<TResult> ExecuteAsync<TResult>(string actionName, Func<CancellationToken, Task<TResult>> action, bool ensureIdempotence = true, CancellationToken cancellationToken = default);
}


