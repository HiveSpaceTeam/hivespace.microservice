using FluentAssertions;
using HiveSpace.Infrastructure.Messaging.Shared.Events.Stores;
using HiveSpace.OrderService.Api.Consumers.Sync;
using HiveSpace.OrderService.Infrastructure.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace HiveSpace.OrderService.Tests.Consumers.Sync;

public class StoreRefSyncConsumerTests
{
    private OrderDbContext CreateDb() =>
        new(new DbContextOptionsBuilder<OrderDbContext>()
            .UseInMemoryDatabase($"order-store-sync-{Guid.NewGuid()}")
            .Options);

    private static StoreRefSyncConsumer MakeConsumer(OrderDbContext db) =>
        new(db, Substitute.For<ILogger<StoreRefSyncConsumer>>());

    [Fact]
    public async Task Consume_StoreCreated_AddsStoreRef()
    {
        var db = CreateDb();
        var consumer = MakeConsumer(db);
        var storeId = Guid.NewGuid();
        var msg = new StoreCreatedIntegrationEvent(storeId, Guid.NewGuid(), "My Store", null, "logo.jpg", null, "123 St");
        var ctx = Substitute.For<ConsumeContext<StoreCreatedIntegrationEvent>>();
        ctx.Message.Returns(msg);
        ctx.CancellationToken.Returns(CancellationToken.None);

        await consumer.Consume(ctx);

        db.StoreRefs.Should().ContainSingle(s => s.Id == storeId);
    }

    [Fact]
    public async Task Consume_StoreUpdated_UpdatesExistingStoreRef()
    {
        var db = CreateDb();
        var consumer = MakeConsumer(db);
        var storeId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var created = new StoreCreatedIntegrationEvent(storeId, ownerId, "Old Store", null, "logo.jpg", null, "Old St");
        var ctx1 = Substitute.For<ConsumeContext<StoreCreatedIntegrationEvent>>();
        ctx1.Message.Returns(created);
        ctx1.CancellationToken.Returns(CancellationToken.None);
        await consumer.Consume(ctx1);

        var updated = new StoreUpdatedIntegrationEvent(storeId, ownerId, "New Store", null, "logo2.jpg", null, "New St");
        var ctx2 = Substitute.For<ConsumeContext<StoreUpdatedIntegrationEvent>>();
        ctx2.Message.Returns(updated);
        ctx2.CancellationToken.Returns(CancellationToken.None);
        await consumer.Consume(ctx2);

        db.StoreRefs.Should().ContainSingle(s => s.Id == storeId);
    }
}
