using HiveSpace.Infrastructure.Messaging.Events;

namespace HiveSpace.Infrastructure.Messaging.Shared.Events.Products;

public record ProductSkuUpdatedEvent(
    int ProductId,
    int SkuId,
    string SkuNo,
    int Quantity,
    decimal Price,
    string Currency
) : IntegrationEvent;
