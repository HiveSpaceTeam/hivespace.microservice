using Microsoft.EntityFrameworkCore;

namespace HiveSpace.Infrastructure.Persistence.Idempotence;

public class IncomingRequestRepository : IIncomingRequestRepository
{
    private readonly DbContext _dbContext;

    public IncomingRequestRepository(DbContext dbContext)
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
} 