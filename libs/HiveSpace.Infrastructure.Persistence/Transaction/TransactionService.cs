using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using HiveSpace.Core.Exceptions;
using HiveSpace.Core.Exceptions.Models;
using HiveSpace.Infrastructure.Persistence.Idempotence;
using HiveSpace.Core.Contexts;

namespace HiveSpace.Infrastructure.Persistence.Transaction;

/// <summary>
/// Provides transaction management services with idempotence checking.
/// </summary>
public class TransactionService<TContext> : ITransactionService, IDisposable
    where TContext : DbContext
{
    private readonly TContext _dbContext;
    private readonly IIncomingRequestRepository _incomingRequestRepository;
    private readonly Guid _requestId;
    private bool _disposedValue;

    public TransactionService(TContext context, IIncomingRequestRepository incomingRequestRepository, IRequestContext requestContext)
    {
        if (!context.Database.IsRelational())
        {
            throw new InvalidOperationException("TransactionService only supports relational databases.");
        }
        _dbContext = context;
        _incomingRequestRepository = incomingRequestRepository;
        _requestId = Guid.TryParse(requestContext.RequestId, out Guid requestId) ? requestId : Guid.NewGuid();
    }

    public async Task IdempotenceCheckAsync()
    {
        if (await _incomingRequestRepository.ExistsAsync(_requestId))
        {
            throw new IdempotenceCheckException(_requestId.ToString());
        }
    }

    public async Task InTransactionScopeAsync(Func<IDbContextTransaction, Task> action, bool performIdempotenceCheck, string actionName)
    {
        var executionStrategy = _dbContext.Database.CreateExecutionStrategy();
        await executionStrategy.ExecuteAsync(async () =>
        {
            using var transaction = await _dbContext.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted);

            if (performIdempotenceCheck)
            {
                await IdempotenceCheckAsync();
            }
            try
            {
                await action?.Invoke(transaction)!;
                _incomingRequestRepository.Add(new IncomingRequest(System.Diagnostics.Activity.Current!.TraceId.ToString(), _requestId, actionName));
                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                await transaction.RollbackAsync();
                _dbContext.ChangeTracker.Clear();
                throw new ConcurrencyException([
                    new Error(CommonErrorCode.ConcurrencyException, null)
                ], ex);
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        });
    }

    public Task OutOfOrderCheckAsync()
    {
        throw new NotImplementedException();
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                _dbContext.Dispose();
            }
            _disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}