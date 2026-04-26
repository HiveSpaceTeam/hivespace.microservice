using HiveSpace.Infrastructure.Messaging.Events;

namespace HiveSpace.Infrastructure.Messaging.Shared.Events.Products;

public record ProductDeletedIntegrationEvent(
    long Id,
    Guid StoreId,
    string Name
) : IntegrationEvent;
