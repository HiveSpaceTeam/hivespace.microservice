using FluentValidation;
using HiveSpace.Core.Exceptions;
using HiveSpace.Core.Exceptions.Models;
using HiveSpace.IdentityService.Core.Exceptions;
using HiveSpace.IdentityService.Core.Features.AccountSessions.Commands;
using Microsoft.Extensions.Configuration;

namespace HiveSpace.IdentityService.Core.Features.ExternalLogins.Commands.StartGoogleChallenge;

public class StartGoogleChallengeCommandValidator : AbstractValidator<StartGoogleChallengeCommand>
{
    public StartGoogleChallengeCommandValidator(IConfiguration configuration)
    {
        RuleFor(c => c.App)
            .Must(AccountSessionValidation.IsBuyerOrSellerApp)
            .WithState(_ => new Error(CommonErrorCode.InvalidArgument, nameof(StartGoogleChallengeCommand.App)));

        RuleFor(c => c.ReturnUrl)
            .Must((command, returnUrl) => AccountSessionValidation.IsSafeGoogleReturnUrl(
                command.App,
                returnUrl,
                GetAllowedOrigin(configuration, command.App)))
            .WithState(_ => new Error(IdentityDomainErrorCode.InvalidReturnUrl, nameof(StartGoogleChallengeCommand.ReturnUrl)))
            .When(c => !string.IsNullOrWhiteSpace(c.ReturnUrl));
    }

    private static string? GetAllowedOrigin(IConfiguration configuration, string app)
        => AccountSessionValidation.IsKnownApp(app)
            ? configuration[$"Authentication:Google:AllowedFrontendOrigins:{AccountSessionHandlerBase.NormalizeApp(app)}"]
            : null;
}
