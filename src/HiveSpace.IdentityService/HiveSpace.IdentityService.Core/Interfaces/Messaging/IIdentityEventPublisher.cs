using HiveSpace.Domain.Shared.Enumerations;
using HiveSpace.IdentityService.Core.Identity;

namespace HiveSpace.IdentityService.Core.Interfaces.Messaging;

public interface IIdentityEventPublisher
{
    Task PublishIdentityUserCreatedAsync(
        ApplicationUser user,
        string? fullName,
        CancellationToken cancellationToken = default);

    Task PublishEmailVerificationRequestedAsync(
        ApplicationUser user,
        string verificationLink,
        DateTime expiresAt,
        Culture locale,
        CancellationToken cancellationToken = default);

    Task PublishEmailVerifiedAsync(
        ApplicationUser user,
        Culture locale,
        CancellationToken cancellationToken = default);
}
