using HiveSpace.Application.Shared.Handlers;
using HiveSpace.Core.Exceptions;
using HiveSpace.Core.Exceptions.Models;
using HiveSpace.Domain.Shared.Enumerations;
using HiveSpace.IdentityService.Core.DomainModels;
using HiveSpace.IdentityService.Core.Exceptions;
using HiveSpace.IdentityService.Core.Features.AccountSessions.Commands;
using HiveSpace.IdentityService.Core.Features.AccountSessions.Dtos;
using HiveSpace.IdentityService.Core.Interfaces.Messaging;
using HiveSpace.IdentityService.Core.Interfaces.Services;
using HiveSpace.IdentityService.Core.Persistence;
using Microsoft.AspNetCore.Identity;
using ConflictException = HiveSpace.Domain.Shared.Exceptions.ConflictException;
using NotFoundException = HiveSpace.Domain.Shared.Exceptions.NotFoundException;

namespace HiveSpace.IdentityService.Core.Features.ExternalLogins.Commands.ConfirmGoogleLink;

public class ConfirmGoogleLinkCommandHandler(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    IIdentityEventPublisher identityEventPublisher,
    IdentityDbContext dbContext,
    IAccountSessionIssuer accountSessionIssuer,
    IPendingGoogleLinkStore pendingGoogleLinkStore)
    : ICommandHandler<ConfirmGoogleLinkCommand, SessionResponse>
{
    public async Task<SessionResponse> Handle(ConfirmGoogleLinkCommand command, CancellationToken cancellationToken)
    {
        var pending = await pendingGoogleLinkStore.GetRequiredAsync(command.LinkToken, cancellationToken);
        var app = AccountSessionHandlerBase.NormalizeApp(command.App);
        if (!string.Equals(pending.App, app, StringComparison.OrdinalIgnoreCase))
            throw new BadRequestException([new Error(CommonErrorCode.InvalidArgument, nameof(command.App))]);

        var user = await userManager.FindByIdAsync(pending.TargetAccountId.ToString())
            ?? throw new NotFoundException(IdentityDomainErrorCode.IdentityUserNotFound, nameof(pending.TargetAccountId));

        var roles = await accountSessionIssuer.ValidateCanIssueAsync(user, app, cancellationToken);

        if (await userManager.FindByLoginAsync(pending.Provider, pending.ProviderKey) is not null)
            throw new ConflictException(IdentityDomainErrorCode.ExternalLoginAlreadyLinked, nameof(pending.Provider));

        var passwordResult = await signInManager.CheckPasswordSignInAsync(user, command.Password, lockoutOnFailure: true);
        if (passwordResult.IsLockedOut)
            throw new ForbiddenException([new Error(IdentityDomainErrorCode.AccountLocked, nameof(user.Email))]);

        if (!passwordResult.Succeeded)
            throw new UnauthorizedException([new Error(IdentityDomainErrorCode.InvalidCredentials, nameof(command.Password))]);

        var addLoginResult = await userManager.AddLoginAsync(user, new UserLoginInfo(
            pending.Provider,
            pending.ProviderKey,
            pending.ProviderDisplayName));
        if (!addLoginResult.Succeeded)
            throw new BadRequestException(addLoginResult.Errors.Select(e => new Error(IdentityDomainErrorCode.ExternalLoginFailed, e.Code)));

        var emailWasUnconfirmed = !user.EmailConfirmed;
        if (string.Equals(user.Email, pending.VerifiedEmail, StringComparison.OrdinalIgnoreCase))
            user.EmailConfirmed = true;

        if (emailWasUnconfirmed && user.EmailConfirmed)
            await identityEventPublisher.PublishEmailVerifiedAsync(user, Culture.Vi, cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);
        await pendingGoogleLinkStore.ClearAsync(cancellationToken);

        return await accountSessionIssuer.IssueAsync(
            user,
            app,
            command.ReturnUrl ?? pending.ReturnUrl,
            updateLastLogin: true,
            roles,
            cancellationToken);
    }
}
