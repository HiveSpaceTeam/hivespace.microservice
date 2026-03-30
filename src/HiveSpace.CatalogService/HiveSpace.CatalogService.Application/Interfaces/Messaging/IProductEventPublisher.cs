using HiveSpace.CatalogService.Domain.Aggregates.ProductAggregate;

namespace HiveSpace.CatalogService.Application.Interfaces.Messaging
{
    public interface IProductEventPublisher
    {
        Task PublishProductCreatedAsync(Product product, CancellationToken cancellationToken = default);
        Task PublishProductUpdatedAsync(Product product, CancellationToken cancellationToken = default);
        Task PublishProductDeletedAsync(Product product, CancellationToken cancellationToken = default);
        Task PublishSkuUpdatedAsync(Product product, CancellationToken cancellationToken = default);
    }
}
