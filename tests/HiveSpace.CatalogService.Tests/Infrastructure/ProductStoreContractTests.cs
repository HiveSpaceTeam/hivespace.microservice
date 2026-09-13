using System.Text.Json;
using FluentAssertions;
using HiveSpace.CatalogService.Application.Products.Dtos;
using HiveSpace.CatalogService.Domain.Aggregates.ProductAggregate;
using HiveSpace.CatalogService.Domain.CatalogImports;
using HiveSpace.CatalogService.Domain.Enums;
using HiveSpace.CatalogService.Infrastructure.Messaging.Publishers;
using HiveSpace.Domain.Shared.Enumerations;
using HiveSpace.Infrastructure.Messaging.Abstractions;
using HiveSpace.Infrastructure.Messaging.Shared.Events.Products;
using NSubstitute;
using Xunit;

namespace HiveSpace.CatalogService.Tests.Infrastructure;

public class ProductStoreContractTests
{
    [Fact]
    public void ProductDetail_UsesStoreIdInJsonContract()
    {
        var storeId = Guid.NewGuid();
        var detail = new ProductDetailDto { StoreId = storeId };

        using var json = JsonDocument.Parse(JsonSerializer.Serialize(detail, new JsonSerializerOptions(JsonSerializerDefaults.Web)));

        json.RootElement.GetProperty("storeId").GetGuid().Should().Be(storeId);
        json.RootElement.TryGetProperty("sellerId", out _).Should().BeFalse();
    }

    [Fact]
    public async Task ProductEvents_PreserveStoreIdAcrossCreateUpdateDeleteAndImport()
    {
        var storeId = Guid.NewGuid();
        var product = Product.CreateProduct("Book", "book", "Description", "Short",
            ProductStatus.Available, storeId, ProductCondition.New, false,
            [], [], [], [], [], DateTimeOffset.UtcNow, "creator");
        var events = Substitute.For<IEventPublisher>();
        var publisher = new ProductEventPublisher(events);
        var bundle = CatalogImportBundle.Create("v1", "tiki", "category", "books", "fingerprint",
            DateTimeOffset.UtcNow, Guid.NewGuid());

        await publisher.PublishProductCreatedAsync(product);
        await publisher.PublishProductUpdatedAsync(product);
        await publisher.PublishProductDeletedAsync(product);
        await publisher.PublishImportedProductsReplicaSyncAsync(bundle, [product]);

        await events.Received(1).PublishAsync(Arg.Is<ProductCreatedIntegrationEvent>(e => e.StoreId == storeId), Arg.Any<CancellationToken>());
        await events.Received(1).PublishAsync(Arg.Is<ProductUpdatedIntegrationEvent>(e => e.StoreId == storeId), Arg.Any<CancellationToken>());
        await events.Received(1).PublishAsync(Arg.Is<ProductDeletedIntegrationEvent>(e => e.StoreId == storeId), Arg.Any<CancellationToken>());
        await events.Received(1).PublishAsync(Arg.Is<ImportedProductsReplicaSyncIntegrationEvent>(e => e.Products.Single().StoreId == storeId), Arg.Any<CancellationToken>());
    }
}
