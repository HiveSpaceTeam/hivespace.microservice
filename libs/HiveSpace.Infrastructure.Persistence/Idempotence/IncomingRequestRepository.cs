using Microsoft.EntityFrameworkCore;

namespace HiveSpace.Infrastructure.Persistence.Idempotence;

/// <summary>
/// Implements IIncomingRequestRepository to manage incoming requests for idempotence checking.
/// </summary>
public class IncomingRequestRepository<TContext> : IIncomingRequestRepository, IDisposable
    where TContext : DbContext
{
    private readonly TContext _dbContext;
    private bool _disposedValue;

    public IncomingRequestRepository(TContext dbContext)
    {
        _dbContext = dbContext;
    }

    public void Add(IncomingRequest incomingRequest)
    {
        _dbContext.Add(incomingRequest);
    }

    public Task<bool> ExistsAsync(Guid requestId)
    {
        return _dbContext.Set<IncomingRequest>()
            .AnyAsync(ir => ir.RequestId == requestId);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
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