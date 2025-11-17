using HiveSpace.Infrastructure.Messaging.Events;

namespace HiveSpace.CatalogService.Application.IntegrationEvents;

public record ProductUpdatedIntegrationEvent : IntegrationEvent
{
    public ProductUpdatedIntegrationEvent(Guid productId, string name, string? description, string updatedBy)
    {
        ProductId = productId;
        Name = name;
        Description = description;
        UpdatedBy = updatedBy;
    }

    public ProductUpdatedIntegrationEvent()
        : this(Guid.Empty, string.Empty, null, string.Empty)
    {
    }

    public Guid ProductId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string UpdatedBy { get; init; } = string.Empty;
}

