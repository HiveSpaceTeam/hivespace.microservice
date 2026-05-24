using HiveSpace.IdentityService.Core.Features.Roles.Commands.AssignSellerRole;
using HiveSpace.Infrastructure.Messaging.Shared.Events.Stores;
using MassTransit;
using MediatR;

namespace HiveSpace.IdentityService.Api.Consumers;

public class StoreCreatedConsumer(IMediator mediator) : IConsumer<StoreCreatedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<StoreCreatedIntegrationEvent> context)
    {
        var message = context.Message;
        await mediator.Send(new AssignSellerRoleCommand(message.OwnerId, message.Id), context.CancellationToken);
    }
}
