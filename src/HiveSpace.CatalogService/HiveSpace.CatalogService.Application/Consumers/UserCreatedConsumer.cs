using HiveSpace.UserService.Application.IntegrationEvents;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace HiveSpace.CatalogService.Application.Consumers;

/// <summary>
/// Sample RabbitMQ consumer that reacts to user lifecycle events.
/// </summary>
public class UserCreatedConsumer : IConsumer<UserCreatedIntegrationEvent>
{
    private readonly ILogger<UserCreatedConsumer> _logger;

    public UserCreatedConsumer(ILogger<UserCreatedConsumer> logger)
    {
        _logger = logger;
    }

    public Task Consume(ConsumeContext<UserCreatedIntegrationEvent> context)
    {
        _logger.LogInformation("User created event received for {UserId} with email {Email}", context.Message.UserId, context.Message.Email);
        return Task.CompletedTask;
    }
}

