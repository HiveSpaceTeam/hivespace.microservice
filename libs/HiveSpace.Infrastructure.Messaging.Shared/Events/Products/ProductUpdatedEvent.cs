using HiveSpace.Domain.Shared.Enumerations;
using HiveSpace.Infrastructure.Messaging.Events;

namespace HiveSpace.Infrastructure.Messaging.Shared.Events.Products;

public record ProductUpdatedEvent(
    long Id,
    Guid StoreId,
    string Name,
    string? ThumbnailUrl,
    ProductStatus Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt
) : IntegrationEvent;
