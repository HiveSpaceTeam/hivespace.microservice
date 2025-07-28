using HiveSpace.Infrastructure.Messaging.Events;

namespace HiveSpace.Infrastructure.Persistence.Outbox;


public interface IOutboxRepository
{
    Task<IEnumerable<OutboxMessage>> RetrieveMessagesPendingToPublishAsync();
    void AddOutboxMessage(IntegrationEvent @event);
    void AddBulkOutboxMessage(IEnumerable<IntegrationEvent> @event);
    Task MarkMessageAsPublishedAsync(Guid messageId);
    Task MarkMessageAsInProgressAsync(Guid messageId);
    Task MarkMessageAsFailedAsync(Guid messageId);
}