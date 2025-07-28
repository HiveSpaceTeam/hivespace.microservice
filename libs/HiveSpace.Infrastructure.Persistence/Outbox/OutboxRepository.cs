using Microsoft.EntityFrameworkCore;
using HiveSpace.Infrastructure.Messaging.Events;

namespace HiveSpace.Infrastructure.Persistence.Outbox;

/// <summary>
/// Implements IOutboxRepository to add domain events to the OutboxMessage table and manage their state.
/// </summary>
public class OutboxRepository<TContext> : IOutboxRepository, IDisposable
    where TContext : DbContext
{
    private readonly TContext _dbContext;
    private bool _disposedValue;

    public OutboxRepository(TContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<OutboxMessage>> RetrieveMessagesPendingToPublishAsync()
    {
        var result = await _dbContext.Set<OutboxMessage>()
            .Where(m => m.State == EventStateEnum.NotPublished)
            .ToListAsync();

        return result.OrderBy(m => m.EventCreationTime);
    }
    
    public void AddOutboxMessage(IntegrationEvent @event)
    {
        var outboxMessage = new OutboxMessage(@event, Guid.NewGuid());
        _dbContext.Set<OutboxMessage>().Add(outboxMessage);
        // Note: Do not call SaveChanges here - let the calling code handle it
    }
    
    public void AddBulkOutboxMessage(IEnumerable<IntegrationEvent> @event)
    {
        foreach (var item in @event)
        {
            AddOutboxMessage(item);
        }
        // Note: Do not call SaveChanges here - let the calling code handle it
    }

    public Task MarkMessageAsPublishedAsync(Guid messageId)
        => UpdateMessageStatus(messageId, EventStateEnum.Published);

    public Task MarkMessageAsInProgressAsync(Guid messageId)
        => UpdateMessageStatus(messageId, EventStateEnum.InProgress);

    public Task MarkMessageAsFailedAsync(Guid messageId)
        => UpdateMessageStatus(messageId, EventStateEnum.PublishedFailed);

    private async Task UpdateMessageStatus(Guid messageId, EventStateEnum status)
    {
        var message = await _dbContext.Set<OutboxMessage>().SingleAsync(m => m.EventId == messageId);
        message.State = status;

        if (status == EventStateEnum.InProgress)
            message.TimesSent++;

        // Save changes immediately for status updates (these are typically called 
        // outside of interceptor context by background services)
        await _dbContext.SaveChangesAsync();
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