using HiveSpace.Infrastructure.Messaging.Events;

namespace HiveSpace.CatalogService.Application.IntegrationEvents;

public record ProductDeletedIntegrationEvent : IntegrationEvent
{
    public ProductDeletedIntegrationEvent(Guid productId, string? deletedBy)
    {
        ProductId = productId;
        DeletedBy = deletedBy;
    }

    public ProductDeletedIntegrationEvent()
        : this(Guid.Empty, null)
    {
    }

    public Guid ProductId { get; init; }
    public string? DeletedBy { get; init; }
}

