using HiveSpace.Infrastructure.Messaging.Events;

namespace HiveSpace.Application.Shared.Events.Products;

public record ProductCreatedIntegrationEvent : IntegrationEvent
{
    public ProductCreatedIntegrationEvent(Guid productId, string name, string? description, string createdBy)
    {
        ProductId = productId;
        Name = name;
        Description = description;
        CreatedBy = createdBy;
    }

    // Parameterless constructor for serialization
    public ProductCreatedIntegrationEvent()
        : this(Guid.Empty, string.Empty, null, string.Empty)
    {
    }

    public Guid ProductId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string CreatedBy { get; init; } = string.Empty;
}

