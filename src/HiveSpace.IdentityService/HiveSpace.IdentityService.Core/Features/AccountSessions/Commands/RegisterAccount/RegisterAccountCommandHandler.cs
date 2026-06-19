using HiveSpace.Application.Shared.Handlers;
using HiveSpace.Core.Exceptions;
using HiveSpace.Core.Exceptions.Models;
using HiveSpace.IdentityService.Core.DomainModels;
using HiveSpace.IdentityService.Core.Exceptions;
using HiveSpace.IdentityService.Core.Features.AccountSessions.Commands;
using HiveSpace.IdentityService.Core.Features.AccountSessions.Dtos;
using HiveSpace.IdentityService.Core.Interfaces.Messaging;
using HiveSpace.IdentityService.Core.Interfaces.Services;
using HiveSpace.IdentityService.Core.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using ConflictException = HiveSpace.Domain.Shared.Exceptions.ConflictException;

namespace HiveSpace.IdentityService.Core.Features.AccountSessions.Commands.RegisterAccount;

public class RegisterAccountCommandHandler(
    UserManager<ApplicationUser> userManager,
    IIdentityEventPublisher identityEventPublisher,
    IEmailVerificationResendCooldownStore resendCooldownStore,
    IdentityDbContext dbContext,
    IConfiguration configuration)
    : ICommandHandler<RegisterAccountCommand, RegisterAccountPendingResponse>
{
    private static readonly TimeSpan InitialResendCooldown = TimeSpan.FromMinutes(1);
    private const string VerificationCallbackPath = "/verify-email-callback";

    public async Task<RegisterAccountPendingResponse> Handle(RegisterAccountCommand command, CancellationToken cancellationToken)
    {
        var app = AccountSessionHandlerBase.NormalizeApp(command.App);
        if (app == "admin")
            throw new ForbiddenException([new Error(IdentityDomainErrorCode.AccountNotAllowed, nameof(command.App))]);

        var email = command.Email.Trim();
        var existingUser = await userManager.FindByEmailAsync(email);
        if (existingUser is not null)
        {
            if (existingUser.Status == UserStatus.Pending && !existingUser.EmailConfirmed)
                throw new ConflictException(IdentityDomainErrorCode.PendingEmailVerification, nameof(command.Email));

            throw new ConflictException(IdentityDomainErrorCode.DuplicateEmail, nameof(command.Email));
        }

        var now = DateTimeOffset.UtcNow;

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FullName = string.IsNullOrWhiteSpace(command.FullName) ? null : command.FullName.Trim(),
            RoleName = "Buyer",
            Status = UserStatus.Pending,
            EmailConfirmed = false,
            CreatedAt = now,
            UpdatedAt = now
        };

        var result = await userManager.CreateAsync(user, command.Password);
        if (!result.Succeeded)
            throw new BadRequestException(result.Errors.Select(e => new Error(IdentityDomainErrorCode.IdentityUserCreationFailed, e.Code)));

        var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
        var encodedToken = WebEncoders.Base64UrlEncode(System.Text.Encoding.UTF8.GetBytes(token));
        var callbackUrl = BuildVerificationCallbackUrl(app);
        var verificationLink = $"{callbackUrl}?userId={Uri.EscapeDataString(user.Id.ToString())}&token={encodedToken}";
        if (!string.IsNullOrWhiteSpace(command.ReturnUrl))
            verificationLink += $"&returnUrl={Uri.EscapeDataString(command.ReturnUrl)}";

        await identityEventPublisher.PublishEmailVerificationRequestedAsync(
            user,
            verificationLink,
            now.UtcDateTime.AddHours(24),
            ResolveCulture(command.Culture),
            cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        await resendCooldownStore.SetCooldownAsync(email.ToUpperInvariant(), now.Add(InitialResendCooldown), cancellationToken);

        return CreatePendingResponse(user.Email!, app, now.Add(InitialResendCooldown));
    }

    private static RegisterAccountPendingResponse CreatePendingResponse(
        string email,
        string app,
        DateTimeOffset canResendAt)
    {
        return new RegisterAccountPendingResponse(
            MaskEmail(email),
            app,
            canResendAt);
    }

    private string BuildVerificationCallbackUrl(string app)
    {
        var origin = configuration[$"FrontendRedirects:{app}:Origin"];
        if (string.IsNullOrWhiteSpace(origin))
            origin = configuration["DefaultRedirectUrl"];
        if (string.IsNullOrWhiteSpace(origin))
            throw new ConfigurationException([new Error(IdentityDomainErrorCode.InvalidConfiguration, $"FrontendRedirects:{app}:Origin")]);

        return new Uri(new Uri(origin.TrimEnd('/') + "/"), VerificationCallbackPath.TrimStart('/')).ToString();
    }

    private static string MaskEmail(string email)
    {
        var atIndex = email.IndexOf('@');
        if (atIndex <= 1)
            return email;

        var localPart = email[..atIndex];
        var domainPart = email[atIndex..];

        if (localPart.Length <= 2)
            return $"{localPart[0]}*{domainPart}";

        return $"{localPart[0]}{new string('*', localPart.Length - 2)}{localPart[^1]}{domainPart}";
    }

    private static HiveSpace.Domain.Shared.Enumerations.Culture ResolveCulture(string? cultureCode)
    {
        if (string.IsNullOrWhiteSpace(cultureCode))
            return HiveSpace.Domain.Shared.Enumerations.Culture.Vi;

        return cultureCode.Trim().ToLowerInvariant() == "en"
            ? HiveSpace.Domain.Shared.Enumerations.Culture.En
            : HiveSpace.Domain.Shared.Enumerations.Culture.Vi;
    }
}
