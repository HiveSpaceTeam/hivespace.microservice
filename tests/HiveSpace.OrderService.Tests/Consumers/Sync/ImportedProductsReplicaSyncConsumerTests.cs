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

public class ImportedProductsReplicaSyncConsumerTests
{
    private OrderDbContext CreateDb() =>
        new(new DbContextOptionsBuilder<OrderDbContext>()
            .UseInMemoryDatabase($"order-import-sync-{Guid.NewGuid()}")
            .Options);

    private static ImportedProductsReplicaSyncConsumer MakeConsumer(OrderDbContext db) =>
        new(db, Substitute.For<ILogger<ImportedProductsReplicaSyncConsumer>>());

    [Fact]
    public async Task Consume_BatchEvent_CreatesProductRefsAndSkuRefs()
    {
        var db = CreateDb();
        var consumer = MakeConsumer(db);
        var message = CreateMessage();
        var context = Substitute.For<ConsumeContext<ImportedProductsReplicaSyncIntegrationEvent>>();
        context.Message.Returns(message);
        context.CancellationToken.Returns(CancellationToken.None);

        await consumer.Consume(context);

        db.ProductRefs.Should().ContainSingle(x => x.Id == 101L && x.Name == "Widget");
        db.SkuRefs.Should().ContainSingle(x => x.Id == 1001L && x.ProductId == 101L);
        db.SkuRefs.Should().ContainSingle(x => x.Id == 1002L && x.ProductId == 101L);
    }

    [Fact]
    public async Task Consume_BatchEvent_WhenReplayed_UpdatesExistingReplicaRows()
    {
        var db = CreateDb();
        var consumer = MakeConsumer(db);
        var createContext = Substitute.For<ConsumeContext<ImportedProductsReplicaSyncIntegrationEvent>>();
        createContext.Message.Returns(CreateMessage());
        createContext.CancellationToken.Returns(CancellationToken.None);
        await consumer.Consume(createContext);

        var updatedMessage = new ImportedProductsReplicaSyncIntegrationEvent(
            Guid.NewGuid(),
            "tiki",
            "sha256:updated",
            DateTimeOffset.UtcNow,
            [
                new ImportedProductReplicaSyncItem(
                    101L,
                    Guid.NewGuid(),
                    "Widget v2",
                    "https://cdn.example.com/widget-v2.png",
                    ProductStatus.Unpublish,
                    [
                        new ImportedSkuReplicaSyncItem(1001L, 101L, "SKU-1", "Blue Updated", 150_000L, "VND", "https://cdn.example.com/sku-1b.png")
                    ])
            ]);
        var updateContext = Substitute.For<ConsumeContext<ImportedProductsReplicaSyncIntegrationEvent>>();
        updateContext.Message.Returns(updatedMessage);
        updateContext.CancellationToken.Returns(CancellationToken.None);

        await consumer.Consume(updateContext);

        db.ProductRefs.Should().ContainSingle(x => x.Id == 101L && x.Name == "Widget v2");
        db.SkuRefs.Should().Contain(x => x.Id == 1001L && x.Price == 150_000L && x.SkuName == "Blue Updated");
        db.SkuRefs.Should().ContainSingle(x => x.Id == 1002L);
    }

    private static ImportedProductsReplicaSyncIntegrationEvent CreateMessage()
        => new(
            Guid.NewGuid(),
            "tiki",
            "sha256:test",
            DateTimeOffset.UtcNow,
            [
                new ImportedProductReplicaSyncItem(
                    101L,
                    Guid.NewGuid(),
                    "Widget",
                    "https://cdn.example.com/widget.png",
                    ProductStatus.Available,
                    [
                        new ImportedSkuReplicaSyncItem(1001L, 101L, "SKU-1", "Blue", 125_000L, "VND", "https://cdn.example.com/sku-1.png"),
                        new ImportedSkuReplicaSyncItem(1002L, 101L, "SKU-2", "Red", 130_000L, "VND", "https://cdn.example.com/sku-2.png")
                    ])
            ]);
}
