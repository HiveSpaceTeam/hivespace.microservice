using HiveSpace.Application.Shared.Events.Users;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace HiveSpace.CatalogService.Application.Consumers;

/// <summary>
/// Sample RabbitMQ consumer that reacts to user lifecycle events.
/// </summary>
public class UserCreatedConsumer : IConsumer<UserCreatedIntegrationEvent>
{

    public UserCreatedConsumer(ILogger<UserCreatedConsumer> logger)
    {
    }

    public Task Consume(ConsumeContext<UserCreatedIntegrationEvent> context)
    {
        return Task.CompletedTask;
    }
}

