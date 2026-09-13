using HiveSpace.CatalogService.Application.Interfaces.Messaging;
using HiveSpace.CatalogService.Domain.Aggregates.ProductAggregate;
using HiveSpace.CatalogService.Domain.CatalogImports;
using HiveSpace.Infrastructure.Messaging.Abstractions;
using HiveSpace.Infrastructure.Messaging.Shared.Events.Products;

namespace HiveSpace.CatalogService.Infrastructure.Messaging.Publishers;

public class ProductEventPublisher(IEventPublisher eventPublisher) : IProductEventPublisher
{
    private readonly IEventPublisher _eventPublisher = eventPublisher;

    public Task PublishProductCreatedAsync(Product product, CancellationToken cancellationToken = default)
    {
        var sku = product.Skus.FirstOrDefault();
        var image = sku?.Images.FirstOrDefault();

        var @event = new ProductCreatedIntegrationEvent(
            product.Id,
            product.StoreId,
            product.Name,
            image?.FileId,
            product.Status,
            product.CreatedAt,
            product.UpdatedAt
        );
        return _eventPublisher.PublishAsync(@event, cancellationToken);
    }

    public Task PublishProductUpdatedAsync(Product product, CancellationToken cancellationToken = default)
    {
        var sku = product.Skus.FirstOrDefault();
        var image = sku?.Images.FirstOrDefault();

        var @event = new ProductUpdatedIntegrationEvent(
            product.Id,
            product.StoreId,
            product.Name,
            image?.FileId,
            product.Status,
            product.CreatedAt,
            product.UpdatedAt
        );
        return _eventPublisher.PublishAsync(@event, cancellationToken);
    }

    public Task PublishProductDeletedAsync(Product product, CancellationToken cancellationToken = default)
    {
        var @event = new ProductDeletedIntegrationEvent(
            product.Id,
            product.StoreId,
            product.Name
        );
        return _eventPublisher.PublishAsync(@event, cancellationToken);
    }

    public Task PublishSkuUpdatedAsync(Product product, CancellationToken cancellationToken = default)
    {
        var tasks = product.Skus.Select(sku => new ProductSkuUpdatedIntegrationEvent(
            product.Id,
            sku.Id,
            sku.SkuNo,
            string.Join(", ", sku.SkuVariants.Select(v => v.Value)),
            sku.Quantity,
            sku.Price.Amount,
            sku.Price.Currency.ToString(),
            sku.Images.FirstOrDefault()?.ImageUrl
        )).Select(evt => _eventPublisher.PublishAsync(evt, cancellationToken));

        return Task.WhenAll(tasks);
    }

    public Task PublishImportedProductsReplicaSyncAsync(
        CatalogImportBundle bundle,
        IReadOnlyCollection<Product> products,
        CancellationToken cancellationToken = default)
    {
        var @event = new ImportedProductsReplicaSyncIntegrationEvent(
            bundle.Id,
            bundle.SourceSystem,
            bundle.SourceFingerprint,
            DateTimeOffset.UtcNow,
            products.Select(product => new ImportedProductReplicaSyncItem(
                product.Id,
                product.StoreId,
                product.Name,
                product.ThumbnailUrl,
                product.Status,
                product.Skus.Select(sku => new ImportedSkuReplicaSyncItem(
                    sku.Id,
                    product.Id,
                    sku.SkuNo,
                    string.Join(", ", sku.SkuVariants.Select(variant => variant.Value)),
                    sku.Price.Amount,
                    sku.Price.Currency.ToString(),
                    sku.Images.FirstOrDefault()?.ImageUrl))
                .ToList()))
            .ToList());

        return _eventPublisher.PublishAsync(@event, cancellationToken);
    }
}
