using HiveSpace.Application.Shared.Events.Users;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace HiveSpace.UserService.Infrastructure.Messaging.Consumers;

public class UserAuditConsumer : IConsumer<UserUpdatedIntegrationEvent>
{
    private readonly ILogger<UserAuditConsumer> _logger;

    public UserAuditConsumer(ILogger<UserAuditConsumer> logger)
    {
        _logger = logger;
    }

    public Task Consume(ConsumeContext<UserUpdatedIntegrationEvent> context)
    {
        _logger.LogInformation("Audit captured user update {UserId}", context.Message.UserId);
        return Task.CompletedTask;
    }
}

