using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.Infrastructure.Messaging.Shared.Events.Users;
using HiveSpace.UserService.Domain.Aggregates.User;
using HiveSpace.UserService.Domain.Exceptions;
using HiveSpace.UserService.Infrastructure.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace HiveSpace.UserService.Api.Consumers;

public class IdentityUserReadyConsumer(UserDbContext dbContext) : IConsumer<IdentityUserReadyIntegrationEvent>
{
    public async Task Consume(ConsumeContext<IdentityUserReadyIntegrationEvent> context)
    {
        var message = context.Message;
        var cancellationToken = context.CancellationToken;

        if (message.UserId == Guid.Empty)
            throw new InvalidFieldException(UserDomainErrorCode.InvalidField, nameof(message.UserId));

        if (string.IsNullOrWhiteSpace(message.Email))
            throw new InvalidFieldException(UserDomainErrorCode.InvalidEmail, nameof(message.Email));

        var exists = await dbContext.Users.AnyAsync(u => u.Id == message.UserId, cancellationToken);
        if (exists)
            return;

        var userName = string.IsNullOrWhiteSpace(message.UserName)
            ? message.Email.Trim()
            : message.UserName.Trim();
        var fullName = string.IsNullOrWhiteSpace(message.FullName)
            ? userName
            : message.FullName.Trim();
        var email = message.Email.Trim();

        var profile = User.CreateProfile(
            message.UserId,
            Email.Create(email),
            userName,
            fullName,
            createdAt: message.ReadyAt == default
                ? DateTimeOffset.UtcNow
                : new DateTimeOffset(DateTime.SpecifyKind(message.ReadyAt, DateTimeKind.Utc)));

        dbContext.Users.Add(profile);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
