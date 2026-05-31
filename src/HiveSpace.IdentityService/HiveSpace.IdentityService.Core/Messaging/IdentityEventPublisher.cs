using HiveSpace.Domain.Shared.Enumerations;
using HiveSpace.IdentityService.Core.DomainModels;
using HiveSpace.IdentityService.Core.Interfaces.Messaging;
using HiveSpace.Infrastructure.Messaging.Shared.Events.Users;
using MassTransit;

namespace HiveSpace.IdentityService.Core.Messaging;

public class IdentityEventPublisher(IPublishEndpoint publishEndpoint) : IIdentityEventPublisher
{
    public Task PublishIdentityUserCreatedAsync(
        ApplicationUser user,
        string? fullName,
        CancellationToken cancellationToken = default)
        => publishEndpoint.Publish(new IdentityUserCreatedIntegrationEvent
        {
            UserId        = user.Id,
            Email         = user.Email!,
            UserName      = user.UserName,
            FullName      = fullName,
            OccurredAt    = DateTime.UtcNow,
            CorrelationId = Guid.NewGuid()
        }, cancellationToken);

    public Task PublishEmailVerificationRequestedAsync(
        ApplicationUser user,
        string verificationLink,
        DateTime expiresAt,
        Culture locale,
        CancellationToken cancellationToken = default)
        => publishEndpoint.Publish(new UserEmailVerificationRequestedIntegrationEvent
        {
            UserId           = user.Id,
            ToEmail          = user.Email!,
            ToName           = user.UserName ?? user.Email!,
            VerificationLink = verificationLink,
            ExpiresAt        = expiresAt,
            Locale           = locale
        }, cancellationToken);

    public Task PublishEmailVerifiedAsync(
        ApplicationUser user,
        Culture locale,
        CancellationToken cancellationToken = default)
        => publishEndpoint.Publish(new UserEmailVerifiedIntegrationEvent
        {
            UserId  = user.Id,
            ToEmail = user.Email!,
            ToName  = user.UserName ?? user.Email!,
            Locale  = locale
        }, cancellationToken);
}
