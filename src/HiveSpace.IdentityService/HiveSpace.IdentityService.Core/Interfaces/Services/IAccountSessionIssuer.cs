using HiveSpace.IdentityService.Core.DomainModels;
using HiveSpace.IdentityService.Core.Features.AccountSessions.Dtos;

namespace HiveSpace.IdentityService.Core.Interfaces.Services;

public interface IAccountSessionIssuer
{
    Task<IReadOnlySet<string>> ValidateCanIssueAsync(
        ApplicationUser user,
        string app,
        CancellationToken cancellationToken = default);

    Task<SessionResponse> IssueAsync(
        ApplicationUser user,
        string app,
        string? returnUrl,
        bool updateLastLogin,
        CancellationToken cancellationToken = default);

    Task<SessionResponse> IssueAsync(
        ApplicationUser user,
        string app,
        string? returnUrl,
        bool updateLastLogin,
        IReadOnlyCollection<string> roles,
        CancellationToken cancellationToken = default);
}
