using HiveSpace.Domain.Shared.Enumerations;
using HiveSpace.IdentityService.Core.DomainModels;

namespace HiveSpace.IdentityService.Core.Interfaces.Messaging;

public interface IIdentityEventPublisher
{
    Task PublishIdentityUserReadyAsync(
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
