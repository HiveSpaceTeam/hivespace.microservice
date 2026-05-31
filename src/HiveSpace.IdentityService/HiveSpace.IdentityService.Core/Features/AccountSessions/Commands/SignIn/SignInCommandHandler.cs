using HiveSpace.Application.Shared.Handlers;
using HiveSpace.Core.Exceptions;
using HiveSpace.Core.Exceptions.Models;
using HiveSpace.IdentityService.Core.DomainModels;
using HiveSpace.IdentityService.Core.Exceptions;
using HiveSpace.IdentityService.Core.Features.AccountSessions.Commands;
using HiveSpace.IdentityService.Core.Features.AccountSessions.Dtos;
using HiveSpace.IdentityService.Core.Features.AccountSessions.Services;
using Microsoft.AspNetCore.Identity;

namespace HiveSpace.IdentityService.Core.Features.AccountSessions.Commands.SignIn;

public class SignInCommandHandler(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    ITokenCookieService tokenCookieService,
    ICsrfTokenService csrfTokenService)
    : ICommandHandler<SignInCommand, SessionResponse>
{
    public async Task<SessionResponse> Handle(SignInCommand command, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(command.Email.Trim());
        if (user is null)
        {
            throw new UnauthorizedException([new Error(IdentityDomainErrorCode.InvalidCredentials, nameof(command.Email))]);
        }

        if (user.Status != UserStatus.Active)
        {
            throw new ForbiddenException([new Error(IdentityDomainErrorCode.AccountInactive, nameof(command.Email))]);
        }

        var roles = await AccountSessionHandlerBase.GetRolesAsync(userManager, user);
        if (!AccountSessionHandlerBase.UserCanAccessApp(user, command.App, roles))
        {
            throw new ForbiddenException([new Error(IdentityDomainErrorCode.AccountNotAllowed, nameof(command.App))]);
        }

        var result = await signInManager.CheckPasswordSignInAsync(user, command.Password, lockoutOnFailure: true);
        if (result.IsLockedOut)
        {
            throw new ForbiddenException([new Error(IdentityDomainErrorCode.AccountLocked, nameof(command.Email))]);
        }

        if (!result.Succeeded)
        {
            throw new UnauthorizedException([new Error(IdentityDomainErrorCode.InvalidCredentials, nameof(command.Email))]);
        }

        user.LastLoginAt = DateTimeOffset.UtcNow;
        user.UpdatedAt = DateTimeOffset.UtcNow;
        await userManager.UpdateAsync(user);

        var issuedSession = await tokenCookieService.IssueAsync(user, command.App, cancellationToken);
        var csrfToken = csrfTokenService.Issue(issuedSession.SessionId, issuedSession.RefreshExpiresAt);
        var sessionUser = AccountSessionHandlerBase.ToSessionUser(user, roles);

        return new SessionResponse(
            sessionUser,
            issuedSession.AccessExpiresAt,
            issuedSession.RefreshExpiresAt,
            csrfToken,
            command.ReturnUrl);
    }
}
