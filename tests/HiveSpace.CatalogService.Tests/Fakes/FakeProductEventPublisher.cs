using HiveSpace.CatalogService.Application.Interfaces.Messaging;
using HiveSpace.CatalogService.Domain.Aggregates.ProductAggregate;

namespace HiveSpace.CatalogService.Tests.Fakes;

public class FakeProductEventPublisher : IProductEventPublisher
{
    public Task PublishProductCreatedAsync(Product product, CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task PublishProductUpdatedAsync(Product product, CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task PublishProductDeletedAsync(Product product, CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task PublishSkuUpdatedAsync(Product product, CancellationToken cancellationToken = default) => Task.CompletedTask;
}
