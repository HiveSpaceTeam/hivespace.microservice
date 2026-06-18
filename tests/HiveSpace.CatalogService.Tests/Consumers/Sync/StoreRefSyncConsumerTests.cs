using HiveSpace.CatalogService.Api.Consumers.Sync;
using HiveSpace.CatalogService.Domain.Aggregates.External;
using HiveSpace.CatalogService.Domain.Repositories.External;
using HiveSpace.Infrastructure.Messaging.Shared.Events.Stores;
using MassTransit;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace HiveSpace.CatalogService.Tests.Consumers.Sync;

public class StoreRefSyncConsumerTests
{
    private readonly IStoreRefRepository _storeRefs = Substitute.For<IStoreRefRepository>();
    private readonly ILogger<StoreRefSyncConsumer> _logger = Substitute.For<ILogger<StoreRefSyncConsumer>>();
    private readonly StoreRefSyncConsumer _consumer;

    public StoreRefSyncConsumerTests()
    {
        _consumer = new StoreRefSyncConsumer(_storeRefs, _logger);
    }

    [Fact]
    public async Task Consume_WhenStoreNotExists_AddsNewStoreRef()
    {
        var storeId = Guid.NewGuid();
        var msg = new StoreCreatedIntegrationEvent(storeId, Guid.NewGuid(), "Shop A", null, "logo.jpg", null, "123 St");
        var ctx = Substitute.For<ConsumeContext<StoreCreatedIntegrationEvent>>();
        ctx.Message.Returns(msg);
        ctx.CancellationToken.Returns(CancellationToken.None);
        _storeRefs.GetByIdAsync(storeId, Arg.Any<CancellationToken>()).Returns((StoreRef?)null);

        await _consumer.Consume(ctx);

        await _storeRefs.Received(1).AddAsync(Arg.Is<StoreRef>(s => s.Id == storeId), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Consume_WhenStoreExists_UpdatesExistingStoreRef()
    {
        var storeId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;
        var existing = new StoreRef(storeId, Guid.NewGuid(), "Old Shop", null, null, "Old St", now, now);
        var msg = new StoreCreatedIntegrationEvent(storeId, Guid.NewGuid(), "New Shop", null, "logo.jpg", null, "New St");
        var ctx = Substitute.For<ConsumeContext<StoreCreatedIntegrationEvent>>();
        ctx.Message.Returns(msg);
        ctx.CancellationToken.Returns(CancellationToken.None);
        _storeRefs.GetByIdAsync(storeId, Arg.Any<CancellationToken>()).Returns(existing);

        await _consumer.Consume(ctx);

        await _storeRefs.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
