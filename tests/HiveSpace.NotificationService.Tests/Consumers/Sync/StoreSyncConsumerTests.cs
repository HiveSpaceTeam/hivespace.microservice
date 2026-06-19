using HiveSpace.Infrastructure.Messaging.Shared.Events.Stores;
using HiveSpace.NotificationService.Api.Consumers.Sync;
using HiveSpace.NotificationService.Core.DomainModels.External;
using HiveSpace.NotificationService.Core.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace HiveSpace.NotificationService.Tests.Consumers.Sync;

public class StoreSyncConsumerTests
{
    private readonly IUserRefRepository _userRefs = Substitute.For<IUserRefRepository>();
    private readonly ILogger<StoreSyncConsumer> _logger = Substitute.For<ILogger<StoreSyncConsumer>>();
    private readonly StoreSyncConsumer _consumer;

    public StoreSyncConsumerTests()
    {
        _consumer = new StoreSyncConsumer(_userRefs, _logger);
    }

    [Fact]
    public async Task Consume_WhenOwnerNotFound_SkipsUpsert()
    {
        var msg = new StoreCreatedIntegrationEvent(Guid.NewGuid(), Guid.NewGuid(), "Shop", null, "logo.jpg", null, "123 St");
        var ctx = Substitute.For<ConsumeContext<StoreCreatedIntegrationEvent>>();
        ctx.Message.Returns(msg);
        ctx.CancellationToken.Returns(CancellationToken.None);
        _userRefs.GetByIdAsync(msg.OwnerId, Arg.Any<CancellationToken>()).Returns((UserRef?)null);

        await _consumer.Consume(ctx);

        await _userRefs.DidNotReceive().UpsertAsync(Arg.Any<UserRef>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Consume_WhenOwnerFound_UpdatesStoreAndUpserts()
    {
        var ownerId = Guid.NewGuid();
        var storeId = Guid.NewGuid();
        var userRef = UserRef.Create(ownerId, "owner@example.com", "Owner");
        var msg = new StoreCreatedIntegrationEvent(storeId, ownerId, "Shop A", null, "logo.jpg", null, "123 St");
        var ctx = Substitute.For<ConsumeContext<StoreCreatedIntegrationEvent>>();
        ctx.Message.Returns(msg);
        ctx.CancellationToken.Returns(CancellationToken.None);
        _userRefs.GetByIdAsync(ownerId, Arg.Any<CancellationToken>()).Returns(userRef);

        await _consumer.Consume(ctx);

        await _userRefs.Received(1).UpsertAsync(Arg.Is<UserRef>(u => u.StoreId == storeId), Arg.Any<CancellationToken>());
    }
}
