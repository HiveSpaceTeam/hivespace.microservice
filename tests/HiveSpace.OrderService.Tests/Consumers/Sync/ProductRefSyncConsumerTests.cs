using FluentAssertions;
using HiveSpace.Domain.Shared.Enumerations;
using HiveSpace.Infrastructure.Messaging.Shared.Events.Products;
using HiveSpace.OrderService.Api.Consumers.Sync;
using HiveSpace.OrderService.Infrastructure.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace HiveSpace.OrderService.Tests.Consumers.Sync;

public class ProductRefSyncConsumerTests
{
    private OrderDbContext CreateDb() =>
        new(new DbContextOptionsBuilder<OrderDbContext>()
            .UseInMemoryDatabase($"order-sync-{Guid.NewGuid()}")
            .Options);

    private static ProductRefSyncConsumer MakeConsumer(OrderDbContext db) =>
        new(db, Substitute.For<ILogger<ProductRefSyncConsumer>>());

    [Fact]
    public async Task Consume_ProductCreated_AddsProductRef()
    {
        var db = CreateDb();
        var consumer = MakeConsumer(db);
        var msg = new ProductCreatedIntegrationEvent(1L, Guid.NewGuid(), "Widget", null, ProductStatus.Available, DateTimeOffset.UtcNow, null);
        var ctx = Substitute.For<ConsumeContext<ProductCreatedIntegrationEvent>>();
        ctx.Message.Returns(msg);
        ctx.CancellationToken.Returns(CancellationToken.None);

        await consumer.Consume(ctx);

        db.ProductRefs.Should().ContainSingle(p => p.Id == 1L && p.Name == "Widget");
    }

    [Fact]
    public async Task Consume_ProductUpdated_UpdatesExistingProductRef()
    {
        var db = CreateDb();
        var consumer = MakeConsumer(db);
        var storeId = Guid.NewGuid();
        var created = new ProductCreatedIntegrationEvent(2L, storeId, "Old Name", null, ProductStatus.Available, DateTimeOffset.UtcNow, null);
        var ctx1 = Substitute.For<ConsumeContext<ProductCreatedIntegrationEvent>>();
        ctx1.Message.Returns(created);
        ctx1.CancellationToken.Returns(CancellationToken.None);
        await consumer.Consume(ctx1);

        var updated = new ProductUpdatedIntegrationEvent(2L, storeId, "New Name", null, ProductStatus.Available, DateTimeOffset.UtcNow, null);
        var ctx2 = Substitute.For<ConsumeContext<ProductUpdatedIntegrationEvent>>();
        ctx2.Message.Returns(updated);
        ctx2.CancellationToken.Returns(CancellationToken.None);
        await consumer.Consume(ctx2);

        db.ProductRefs.Single(p => p.Id == 2L).Name.Should().Be("New Name");
    }

    [Fact]
    public async Task Consume_ProductDeleted_RemovesProductRef()
    {
        var db = CreateDb();
        var consumer = MakeConsumer(db);
        var create = new ProductCreatedIntegrationEvent(3L, Guid.NewGuid(), "ToDelete", null, ProductStatus.Available, DateTimeOffset.UtcNow, null);
        var ctx1 = Substitute.For<ConsumeContext<ProductCreatedIntegrationEvent>>();
        ctx1.Message.Returns(create);
        ctx1.CancellationToken.Returns(CancellationToken.None);
        await consumer.Consume(ctx1);

        var delete = new ProductDeletedIntegrationEvent(3L, Guid.NewGuid(), "ToDelete");
        var ctx2 = Substitute.For<ConsumeContext<ProductDeletedIntegrationEvent>>();
        ctx2.Message.Returns(delete);
        ctx2.CancellationToken.Returns(CancellationToken.None);
        await consumer.Consume(ctx2);

        db.ProductRefs.Should().NotContain(p => p.Id == 3L);
    }

    [Fact]
    public async Task Consume_ProductSkuUpdated_UpsertSkuRef()
    {
        var db = CreateDb();
        var consumer = MakeConsumer(db);
        var msg = new ProductSkuUpdatedIntegrationEvent(10, 100, "SKU-001", "Blue L", 5, 50_000L, "VND");
        var ctx = Substitute.For<ConsumeContext<ProductSkuUpdatedIntegrationEvent>>();
        ctx.Message.Returns(msg);
        ctx.CancellationToken.Returns(CancellationToken.None);

        await consumer.Consume(ctx);

        db.SkuRefs.Should().ContainSingle(s => s.Id == 100L);
    }

    [Fact]
    public async Task Consume_ProductDeleted_WhenNotFound_DoesNothing()
    {
        var db = CreateDb();
        var consumer = MakeConsumer(db);
        var delete = new ProductDeletedIntegrationEvent(99L, Guid.NewGuid(), "Ghost");
        var ctx = Substitute.For<ConsumeContext<ProductDeletedIntegrationEvent>>();
        ctx.Message.Returns(delete);
        ctx.CancellationToken.Returns(CancellationToken.None);

        await consumer.Consume(ctx);

        db.ProductRefs.Should().BeEmpty();
    }

    [Fact]
    public async Task Consume_ProductSkuUpdated_WhenSkuExists_UpdatesSkuRef()
    {
        var db = CreateDb();
        var consumer = MakeConsumer(db);
        var create = new ProductSkuUpdatedIntegrationEvent(10, 200, "SKU-200", "Red S", 3, 30_000L, "VND");
        var ctx1 = Substitute.For<ConsumeContext<ProductSkuUpdatedIntegrationEvent>>();
        ctx1.Message.Returns(create);
        ctx1.CancellationToken.Returns(CancellationToken.None);
        await consumer.Consume(ctx1);

        var update = new ProductSkuUpdatedIntegrationEvent(10, 200, "SKU-200-V2", "Red S Updated", 5, 40_000L, "VND");
        var ctx2 = Substitute.For<ConsumeContext<ProductSkuUpdatedIntegrationEvent>>();
        ctx2.Message.Returns(update);
        ctx2.CancellationToken.Returns(CancellationToken.None);
        await consumer.Consume(ctx2);

        db.SkuRefs.Single(s => s.Id == 200L).Price.Should().Be(40_000L);
    }
}
