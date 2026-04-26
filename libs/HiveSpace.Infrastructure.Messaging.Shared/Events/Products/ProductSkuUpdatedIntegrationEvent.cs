using HiveSpace.Infrastructure.Messaging.Events;

namespace HiveSpace.Infrastructure.Messaging.Shared.Events.Products;

public record ProductSkuUpdatedIntegrationEvent(
    int ProductId,
    int SkuId,
    string SkuNo,
    string SkuName,
    int Quantity,
    long Price,
    string Currency
) : IntegrationEvent;
