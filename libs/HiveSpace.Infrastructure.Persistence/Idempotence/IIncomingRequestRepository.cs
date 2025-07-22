namespace HiveSpace.Infrastructure.Persistence.Idempotence;

public interface IIncomingRequestRepository
{
    void Add(IncomingRequest incomingRequest);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid requestId);
} 