using HiveSpace.Core.Exceptions;
using HiveSpace.Core.Exceptions.Models;
using HiveSpace.Domain.Shared.Enumerations;
using HiveSpace.IdentityService.Core.DomainModels;
using HiveSpace.IdentityService.Core.Exceptions;
using HiveSpace.IdentityService.Core.Features.AccountSessions.Commands;
using HiveSpace.IdentityService.Core.Features.AccountSessions.Dtos;
using HiveSpace.IdentityService.Core.Interfaces.Services;
using Microsoft.AspNetCore.Identity;

namespace HiveSpace.IdentityService.Core.Features.AccountSessions.Services;

public class AccountSessionIssuer(
    UserManager<ApplicationUser> userManager,
    ITokenCookieService tokenCookieService,
    ICsrfTokenService csrfTokenService)
    : IAccountSessionIssuer
{
    public async Task<IReadOnlySet<string>> ValidateCanIssueAsync(
        ApplicationUser user,
        string app,
        CancellationToken cancellationToken = default)
    {
        if (user.Status != UserStatus.Active)
            throw new ForbiddenException([new Error(IdentityDomainErrorCode.AccountInactive, nameof(user.Email))]);

        if (await userManager.IsLockedOutAsync(user))
            throw new ForbiddenException([new Error(IdentityDomainErrorCode.AccountLocked, nameof(user.Email))]);

        var roles = await AccountSessionHandlerBase.GetRolesAsync(userManager, user);
        if (!AccountSessionHandlerBase.UserCanAccessApp(user, app, roles))
            throw new ForbiddenException([new Error(IdentityDomainErrorCode.AccountNotAllowed, nameof(app))]);

        return roles;
    }

    public async Task<SessionResponse> IssueAsync(
        ApplicationUser user,
        string app,
        string? returnUrl,
        bool updateLastLogin,
        CancellationToken cancellationToken = default)
    {
        var roles = await ValidateCanIssueAsync(user, app, cancellationToken);
        return await IssueAsync(user, app, returnUrl, updateLastLogin, roles, cancellationToken);
    }

    public async Task<SessionResponse> IssueAsync(
        ApplicationUser user,
        string app,
        string? returnUrl,
        bool updateLastLogin,
        IReadOnlyCollection<string> roles,
        CancellationToken cancellationToken = default)
    {
        if (updateLastLogin)
        {
            user.LastLoginAt = DateTimeOffset.UtcNow;
            user.UpdatedAt = DateTimeOffset.UtcNow;
            await userManager.UpdateAsync(user);
        }

        var issuedSession = await tokenCookieService.IssueAsync(user, app, cancellationToken);
        var csrfToken = csrfTokenService.Issue(issuedSession.SessionId, issuedSession.RefreshExpiresAt);
        var sessionUser = AccountSessionHandlerBase.ToSessionUser(user, roles);

        return new SessionResponse(
            sessionUser,
            issuedSession.AccessExpiresAt,
            issuedSession.RefreshExpiresAt,
            csrfToken,
            returnUrl);
    }
}
