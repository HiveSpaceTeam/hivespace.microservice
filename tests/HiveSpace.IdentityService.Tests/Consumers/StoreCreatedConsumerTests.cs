using HiveSpace.IdentityService.Api.Consumers;
using HiveSpace.IdentityService.Core.Features.Roles.Commands.AssignSellerRole;
using HiveSpace.Infrastructure.Messaging.Shared.Events.Stores;
using MassTransit;
using MediatR;
using NSubstitute;
using Xunit;

namespace HiveSpace.IdentityService.Tests.Consumers;

public class StoreCreatedConsumerTests
{
    private readonly IMediator _mediator = Substitute.For<IMediator>();
    private readonly StoreCreatedConsumer _consumer;

    public StoreCreatedConsumerTests()
    {
        _consumer = new StoreCreatedConsumer(_mediator);
    }

    [Fact]
    public async Task Consume_SendsAssignSellerRoleCommand()
    {
        var ownerId = Guid.NewGuid();
        var storeId = Guid.NewGuid();
        var msg = new StoreCreatedIntegrationEvent(storeId, ownerId, "Shop", null, "logo.jpg", null, "123 St");
        var ctx = Substitute.For<ConsumeContext<StoreCreatedIntegrationEvent>>();
        ctx.Message.Returns(msg);
        ctx.CancellationToken.Returns(CancellationToken.None);

        await _consumer.Consume(ctx);

        await _mediator.Received(1).Send(
            Arg.Is<AssignSellerRoleCommand>(c => c.UserId == ownerId && c.StoreId == storeId),
            Arg.Any<CancellationToken>());
    }
}
