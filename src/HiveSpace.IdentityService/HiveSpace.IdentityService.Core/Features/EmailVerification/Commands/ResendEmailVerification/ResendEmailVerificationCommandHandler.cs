using System.Text;
using HiveSpace.Application.Shared.Handlers;
using HiveSpace.Core.Exceptions;
using HiveSpace.Core.Exceptions.Models;
using HiveSpace.Domain.Shared.Enumerations;
using HiveSpace.IdentityService.Core.DomainModels;
using HiveSpace.IdentityService.Core.Exceptions;
using HiveSpace.IdentityService.Core.Features.AccountSessions.Commands;
using HiveSpace.IdentityService.Core.Interfaces.Messaging;
using HiveSpace.IdentityService.Core.Interfaces.Services;
using HiveSpace.IdentityService.Core.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;

namespace HiveSpace.IdentityService.Core.Features.EmailVerification.Commands.ResendEmailVerification;

public class ResendEmailVerificationCommandHandler(
    UserManager<ApplicationUser> userManager,
    IIdentityEventPublisher identityEventPublisher,
    IEmailVerificationResendCooldownStore resendCooldownStore,
    IdentityDbContext dbContext,
    IConfiguration configuration)
    : ICommandHandler<ResendEmailVerificationCommand>
{
    private static readonly TimeSpan ResendCooldown = TimeSpan.FromMinutes(1);
    private const string VerificationCallbackPath = "/verify-email-callback";

    public async Task Handle(ResendEmailVerificationCommand command, CancellationToken cancellationToken)
    {
        var app = AccountSessionHandlerBase.NormalizeApp(command.App);
        if (app == "admin")
            throw new ForbiddenException([new Error(IdentityDomainErrorCode.AccountNotAllowed, nameof(command.App))]);

        var email = command.Email.Trim();
        var normalizedEmail = email.ToUpperInvariant();
        var cooldownEndsAt = await resendCooldownStore.GetCooldownEndsAtAsync(normalizedEmail, cancellationToken);
        if (cooldownEndsAt.HasValue && cooldownEndsAt.Value > DateTimeOffset.UtcNow)
            return;

        var user = await userManager.FindByEmailAsync(email);
        if (user is null || user.EmailConfirmed)
            return;

        var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
        var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
        var callbackUrl = BuildVerificationCallbackUrl(app);
        var verificationLink = $"{callbackUrl}?userId={Uri.EscapeDataString(user.Id.ToString())}&token={encodedToken}";
        if (!string.IsNullOrWhiteSpace(command.ReturnUrl))
            verificationLink += $"&returnUrl={Uri.EscapeDataString(command.ReturnUrl)}";

        var nextCooldownEndsAt = DateTimeOffset.UtcNow.Add(ResendCooldown);

        await identityEventPublisher.PublishEmailVerificationRequestedAsync(
            user,
            verificationLink,
            DateTime.UtcNow.AddHours(24),
            ResolveCulture(command.Culture),
            cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        await resendCooldownStore.SetCooldownAsync(normalizedEmail, nextCooldownEndsAt, cancellationToken);
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

    private static Culture ResolveCulture(string? cultureCode)
    {
        if (string.IsNullOrWhiteSpace(cultureCode))
            return Culture.Vi;

        return cultureCode.Trim().ToLowerInvariant() == "en"
            ? Culture.En
            : Culture.Vi;
    }
}
