using HiveSpace.UserService.Application.IntegrationEvents;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace HiveSpace.UserService.Infrastructure.Messaging.Consumers;

public class UserAnalyticsConsumer : IConsumer<UserCreatedIntegrationEvent>
{
    private readonly ILogger<UserAnalyticsConsumer> _logger;

    public UserAnalyticsConsumer(ILogger<UserAnalyticsConsumer> logger)
    {
        _logger = logger;
    }

    public Task Consume(ConsumeContext<UserCreatedIntegrationEvent> context)
    {
        _logger.LogInformation("Analytics captured user {UserId}", context.Message.UserId);
        return Task.CompletedTask;
    }
}

