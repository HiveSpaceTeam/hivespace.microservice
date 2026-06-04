using HiveSpace.Application.Shared.Handlers;
using HiveSpace.Core.Exceptions;
using HiveSpace.Core.Exceptions.Models;
using HiveSpace.IdentityService.Core.DomainModels;
using HiveSpace.IdentityService.Core.Exceptions;
using HiveSpace.IdentityService.Core.Features.AccountSessions.Dtos;
using HiveSpace.IdentityService.Core.Interfaces.Services;
using Microsoft.AspNetCore.Identity;

namespace HiveSpace.IdentityService.Core.Features.AccountSessions.Commands.SignIn;

public class SignInCommandHandler(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    IAccountSessionIssuer accountSessionIssuer)
    : ICommandHandler<SignInCommand, SessionResponse>
{
    public async Task<SessionResponse> Handle(SignInCommand command, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(command.Email.Trim());
        if (user is null)
        {
            throw new UnauthorizedException([new Error(IdentityDomainErrorCode.InvalidCredentials, nameof(command.Email))]);
        }

        var roles = await accountSessionIssuer.ValidateCanIssueAsync(user, command.App, cancellationToken);

        var result = await signInManager.CheckPasswordSignInAsync(user, command.Password, lockoutOnFailure: true);
        if (result.IsLockedOut)
        {
            throw new ForbiddenException([new Error(IdentityDomainErrorCode.AccountLocked, nameof(command.Email))]);
        }

        if (!result.Succeeded)
        {
            throw new UnauthorizedException([new Error(IdentityDomainErrorCode.InvalidCredentials, nameof(command.Email))]);
        }

        return await accountSessionIssuer.IssueAsync(
            user,
            command.App,
            command.ReturnUrl,
            updateLastLogin: true,
            roles,
            cancellationToken);
    }
}
