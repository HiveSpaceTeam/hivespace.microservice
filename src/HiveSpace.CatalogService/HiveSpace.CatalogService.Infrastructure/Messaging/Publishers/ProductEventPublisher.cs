using HiveSpace.CatalogService.Application.Interfaces.Messaging;
using HiveSpace.CatalogService.Domain.Aggregates.ProductAggregate;
using HiveSpace.Infrastructure.Messaging.Abstractions;
using HiveSpace.Infrastructure.Messaging.Shared.Events.Products;

namespace HiveSpace.CatalogService.Infrastructure.Messaging.Publishers
{
    public class ProductEventPublisher : IProductEventPublisher
    {
        private readonly IEventPublisher _eventPublisher;

        public ProductEventPublisher(IEventPublisher eventPublisher)
        {
            _eventPublisher = eventPublisher;
        }


        public Task PublishProductCreatedAsync(Product product, CancellationToken cancellationToken = default)
        {
            var sku = product.Skus.FirstOrDefault();
            var image = sku?.Images.FirstOrDefault();

            var @event = new ProductCreatedEvent(
                product.Id,
                product.SellerId,
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

            var @event = new ProductUpdatedEvent(
                product.Id,
                product.SellerId,
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
            var @event = new ProductDeletedEvent(
                product.Id,
                product.SellerId,
                product.Name
            );
            return _eventPublisher.PublishAsync(@event, cancellationToken);
        }

        public Task PublishSkuUpdatedAsync(Product product, CancellationToken cancellationToken = default)
        {
            var tasks = product.Skus.Select(sku => new ProductSkuUpdatedEvent(
                product.Id,
                sku.Id,
                sku.SkuNo,
                sku.Quantity,
                sku.Price.Amount,
                sku.Price.Currency.ToString()
            )).Select(evt => _eventPublisher.PublishAsync(evt, cancellationToken));

            return Task.WhenAll(tasks);
        }
    }
}
