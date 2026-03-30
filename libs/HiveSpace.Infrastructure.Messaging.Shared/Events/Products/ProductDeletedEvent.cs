using HiveSpace.Infrastructure.Messaging.Events;

namespace HiveSpace.Infrastructure.Messaging.Shared.Events.Products;

public record ProductDeletedEvent(
    long Id,
    Guid StoreId,
    string Name
) : IntegrationEvent;
