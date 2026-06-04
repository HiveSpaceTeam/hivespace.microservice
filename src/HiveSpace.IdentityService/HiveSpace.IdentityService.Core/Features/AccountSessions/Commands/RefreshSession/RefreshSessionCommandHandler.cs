using HiveSpace.Application.Shared.Handlers;
using HiveSpace.Core.Exceptions;
using HiveSpace.Core.Exceptions.Models;
using HiveSpace.IdentityService.Core.DomainModels;
using HiveSpace.IdentityService.Core.Exceptions;
using HiveSpace.IdentityService.Core.Features.AccountSessions.Commands;
using HiveSpace.IdentityService.Core.Features.AccountSessions.Dtos;
using HiveSpace.IdentityService.Core.Interfaces.Services;
using Microsoft.AspNetCore.Identity;

namespace HiveSpace.IdentityService.Core.Features.AccountSessions.Commands.RefreshSession;

public class RefreshSessionCommandHandler(
    UserManager<ApplicationUser> userManager,
    ITokenCookieService tokenCookieService,
    ICsrfTokenService csrfTokenService)
    : ICommandHandler<RefreshSessionCommand, SessionResponse>
{
    public async Task<SessionResponse> Handle(RefreshSessionCommand command, CancellationToken cancellationToken)
    {
        var currentSession = await tokenCookieService.GetRequiredRefreshSessionAsync(cancellationToken);
        if (currentSession.RefreshExpiresAt <= DateTimeOffset.UtcNow)
            throw new UnauthorizedException([new Error(IdentityDomainErrorCode.SessionExpired, "session")]);

        var user = await userManager.FindByIdAsync(currentSession.UserId.ToString())
            ?? throw new UnauthorizedException([new Error(IdentityDomainErrorCode.InvalidSession, "session")]);

        if (user.Status != UserStatus.Active)
            throw new ForbiddenException([new Error(IdentityDomainErrorCode.AccountInactive, nameof(user.Email))]);

        if (await userManager.IsLockedOutAsync(user))
            throw new ForbiddenException([new Error(IdentityDomainErrorCode.AccountLocked, nameof(user.Email))]);

        var securityStamp = await userManager.GetSecurityStampAsync(user);
        if (!string.Equals(currentSession.SecurityStamp, securityStamp, StringComparison.Ordinal))
            throw new UnauthorizedException([new Error(IdentityDomainErrorCode.InvalidSession, "session")]);

        var roles = await AccountSessionHandlerBase.GetRolesAsync(userManager, user);
        if (!AccountSessionHandlerBase.UserCanAccessApp(user, command.App, roles))
            throw new ForbiddenException([new Error(IdentityDomainErrorCode.AccountNotAllowed, nameof(command.App))]);

        var issuedSession = await tokenCookieService.RefreshAsync(currentSession, user, command.App, cancellationToken);
        var csrfToken = csrfTokenService.Issue(issuedSession.SessionId, issuedSession.RefreshExpiresAt);
        var sessionUser = AccountSessionHandlerBase.ToSessionUser(user, roles);

        return new SessionResponse(
            sessionUser,
            issuedSession.AccessExpiresAt,
            issuedSession.RefreshExpiresAt,
            csrfToken,
            null);
    }
}
