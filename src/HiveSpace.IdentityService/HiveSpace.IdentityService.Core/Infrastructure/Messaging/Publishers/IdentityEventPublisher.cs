using HiveSpace.Domain.Shared.Enumerations;
using HiveSpace.IdentityService.Core.DomainModels;
using HiveSpace.IdentityService.Core.Interfaces.Messaging;
using HiveSpace.Infrastructure.Messaging.Shared.Events.Users;
using MassTransit;

namespace HiveSpace.IdentityService.Core.Infrastructure.Messaging.Publishers;

public class IdentityEventPublisher(IPublishEndpoint publishEndpoint) : IIdentityEventPublisher
{
    public Task PublishIdentityUserReadyAsync(
        ApplicationUser user,
        string? fullName,
        CancellationToken cancellationToken = default)
        => publishEndpoint.Publish(new IdentityUserReadyIntegrationEvent
        {
            UserId   = user.Id,
            Email    = user.Email!,
            UserName = user.UserName,
            FullName = fullName ?? user.FullName,
            ReadyAt  = DateTime.UtcNow
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
            ToName           = user.FullName ?? user.UserName ?? user.Email!,
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
            ToName  = user.FullName ?? user.UserName ?? user.Email!,
            Locale  = locale
        }, cancellationToken);

    public Task PublishOtpChallengeRequestedAsync(
        ApplicationUser user,
        string otpCode,
        DateTimeOffset expiresAt,
        string purpose,
        CancellationToken cancellationToken = default)
        => publishEndpoint.Publish(new UserOtpChallengeRequestedIntegrationEvent
        {
            RecipientEmail = user.Email!,
            OtpCode = otpCode,
            ExpiresAt = expiresAt,
            Purpose = purpose
        }, cancellationToken);
}
