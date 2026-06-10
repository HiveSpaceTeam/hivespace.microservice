using System.Security.Claims;
using HiveSpace.Application.Shared.Handlers;
using HiveSpace.Core.Exceptions;
using HiveSpace.Core.Exceptions.Models;
using HiveSpace.Domain.Shared.Enumerations;
using HiveSpace.IdentityService.Core.DomainModels;
using HiveSpace.IdentityService.Core.Exceptions;
using HiveSpace.IdentityService.Core.Features.AccountSessions.Commands;
using HiveSpace.IdentityService.Core.Features.ExternalLogins.Dtos;
using HiveSpace.IdentityService.Core.Interfaces.Messaging;
using HiveSpace.IdentityService.Core.Interfaces.Services;
using HiveSpace.IdentityService.Core.Persistence;
using Microsoft.AspNetCore.Identity;

namespace HiveSpace.IdentityService.Core.Features.ExternalLogins.Commands.CompleteGoogleCallback;

public class CompleteGoogleCallbackCommandHandler(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    IIdentityEventPublisher identityEventPublisher,
    IdentityDbContext dbContext,
    IAccountSessionIssuer accountSessionIssuer,
    IPendingGoogleLinkStore pendingGoogleLinkStore)
    : ICommandHandler<CompleteGoogleCallbackCommand, GoogleCallbackResult>
{
    public async Task<GoogleCallbackResult> Handle(CompleteGoogleCallbackCommand command, CancellationToken cancellationToken)
    {
        var app = AccountSessionHandlerBase.NormalizeApp(command.App);
        var loginInfo = await signInManager.GetExternalLoginInfoAsync();
        if (loginInfo is null)
            return Failed(command, "google_callback_failed");

        var email = FindClaim(loginInfo.Principal, ClaimTypes.Email, "email");
        var emailVerified = IsTrue(FindClaim(loginInfo.Principal, "email_verified"));
        if (string.IsNullOrWhiteSpace(email))
            return Failed(command, "GoogleEmailMissing");

        if (!emailVerified)
            return Failed(command, "GoogleEmailUnverified");

        var linkedUser = await userManager.FindByLoginAsync(loginInfo.LoginProvider, loginInfo.ProviderKey);
        if (linkedUser is not null)
        {
            await accountSessionIssuer.IssueAsync(
                linkedUser,
                app,
                command.ReturnUrl,
                updateLastLogin: true,
                cancellationToken);
            return SignedIn(command, app);
        }

        var existingUser = await userManager.FindByEmailAsync(email.Trim());
        if (existingUser is not null)
        {
            if (!await userManager.HasPasswordAsync(existingUser))
                return Failed(command, "GoogleLinkFailed");

            var pendingState = await pendingGoogleLinkStore.CreateAsync(new PendingGoogleLinkCreateRequest(
                loginInfo.LoginProvider,
                loginInfo.ProviderKey,
                loginInfo.ProviderDisplayName ?? "Google",
                email.Trim(),
                existingUser.Id,
                app,
                command.ReturnUrl,
                command.Culture,
                DateTimeOffset.UtcNow.AddMinutes(10)), cancellationToken);

            return new GoogleCallbackResult(
                GoogleCallbackOutcome.PendingLink,
                app,
                command.ReturnUrl,
                command.Culture,
                pendingState.LinkToken,
                null);
        }

        var user = new ApplicationUser
        {
            UserName = email.Trim(),
            Email = email.Trim(),
            FullName = FindClaim(loginInfo.Principal, ClaimTypes.Name, "name"),
            EmailConfirmed = true,
            RoleName = "Buyer",
            Status = UserStatus.Active,
            ActivatedAt = DateTimeOffset.UtcNow,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            LastLoginAt = DateTimeOffset.UtcNow
        };

        var createResult = await userManager.CreateAsync(user);
        if (!createResult.Succeeded)
            throw new BadRequestException(createResult.Errors.Select(e => new Error(IdentityDomainErrorCode.IdentityUserCreationFailed, e.Code)));

        var loginResult = await userManager.AddLoginAsync(user, loginInfo);
        if (!loginResult.Succeeded)
            throw new BadRequestException(loginResult.Errors.Select(e => new Error(IdentityDomainErrorCode.ExternalLoginFailed, e.Code)));

        await identityEventPublisher.PublishIdentityUserReadyAsync(
            user,
            user.FullName,
            cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        await accountSessionIssuer.IssueAsync(
            user,
            app,
            command.ReturnUrl,
            updateLastLogin: false,
            cancellationToken);
        return SignedIn(command, app);
    }

    private static GoogleCallbackResult SignedIn(CompleteGoogleCallbackCommand command, string app)
        => new(GoogleCallbackOutcome.SignedIn, app, command.ReturnUrl, command.Culture, null, null);

    private static GoogleCallbackResult Failed(CompleteGoogleCallbackCommand command, string errorCode)
        => new(GoogleCallbackOutcome.Failed, AccountSessionHandlerBase.NormalizeApp(command.App), command.ReturnUrl, command.Culture, null, errorCode);

    private static string? FindClaim(ClaimsPrincipal principal, params string[] claimTypes)
        => claimTypes.Select(type => principal.FindFirstValue(type)).FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));

    private static bool IsTrue(string? value)
        => string.Equals(value, "true", StringComparison.OrdinalIgnoreCase)
            || string.Equals(value, "1", StringComparison.OrdinalIgnoreCase);
}
