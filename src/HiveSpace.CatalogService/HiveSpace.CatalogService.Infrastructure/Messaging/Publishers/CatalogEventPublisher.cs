using HiveSpace.Application.Shared.Events.Products;
using HiveSpace.CatalogService.Application.Interfaces.Messaging;
using HiveSpace.CatalogService.Domain.Aggregates.ProductAggregate;
using HiveSpace.Infrastructure.Messaging.Abstractions;

namespace HiveSpace.CatalogService.Infrastructure.Messaging.Publishers;

public class CatalogEventPublisher : ICatalogEventPublisher
{
    private readonly IEventPublisher _eventPublisher;

    public CatalogEventPublisher(IEventPublisher eventPublisher)
    {
        _eventPublisher = eventPublisher;
    }

    public Task PublishProductCreatedAsync(Product product, CancellationToken cancellationToken = default)
    {
        var integrationEvent = new ProductCreatedIntegrationEvent(product.Id, product.Name, product.Description, product.CreatedBy);
        return _eventPublisher.PublishAsync(integrationEvent, cancellationToken);
    }

    public Task PublishProductUpdatedAsync(Product product, CancellationToken cancellationToken = default)
    {
        var integrationEvent = new ProductUpdatedIntegrationEvent(product.Id, product.Name, product.Description, product.UpdatedBy ?? product.CreatedBy);
        return _eventPublisher.PublishAsync(integrationEvent, cancellationToken);
    }

    public Task PublishProductDeletedAsync(Guid productId, string? deletedBy, CancellationToken cancellationToken = default)
    {
        var integrationEvent = new ProductDeletedIntegrationEvent(productId, deletedBy);
        return _eventPublisher.PublishAsync(integrationEvent, cancellationToken);
    }
}

