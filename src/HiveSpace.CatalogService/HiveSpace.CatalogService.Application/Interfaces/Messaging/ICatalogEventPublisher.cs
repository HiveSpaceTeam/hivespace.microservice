using HiveSpace.CatalogService.Domain.Aggregates.ProductAggregate;

namespace HiveSpace.CatalogService.Application.Interfaces.Messaging;

public interface ICatalogEventPublisher
{
    Task PublishProductCreatedAsync(Product product, CancellationToken cancellationToken = default);
    Task PublishProductUpdatedAsync(Product product, CancellationToken cancellationToken = default);
    Task PublishProductDeletedAsync(Guid productId, string? deletedBy, CancellationToken cancellationToken = default);
}

