using FluentValidation;
using HiveSpace.Core.Exceptions;
using HiveSpace.Core.Exceptions.Models;
using HiveSpace.IdentityService.Core.Exceptions;
using HiveSpace.IdentityService.Core.Features.AccountSessions.Commands;
using Microsoft.Extensions.Configuration;

namespace HiveSpace.IdentityService.Core.Features.ExternalLogins.Commands.ConfirmGoogleLink;

public class ConfirmGoogleLinkCommandValidator : AbstractValidator<ConfirmGoogleLinkCommand>
{
    public ConfirmGoogleLinkCommandValidator(IConfiguration configuration)
    {
        RuleFor(c => c.ConsentAccepted)
            .Equal(true)
            .WithState(_ => new Error(CommonErrorCode.InvalidArgument, nameof(ConfirmGoogleLinkCommand.ConsentAccepted)));
        RuleFor(c => c.Password)
            .NotEmpty()
            .WithState(_ => new Error(CommonErrorCode.Required, nameof(ConfirmGoogleLinkCommand.Password)));
        RuleFor(c => c.App)
            .Must(AccountSessionValidation.IsBuyerOrSellerApp)
            .WithState(_ => new Error(CommonErrorCode.InvalidArgument, nameof(ConfirmGoogleLinkCommand.App)));
        RuleFor(c => c.ReturnUrl)
            .Must((command, returnUrl) => AccountSessionValidation.IsSafeGoogleReturnUrl(
                command.App,
                returnUrl,
                GetAllowedOrigin(configuration, command.App)))
            .WithState(_ => new Error(IdentityDomainErrorCode.InvalidReturnUrl, nameof(ConfirmGoogleLinkCommand.ReturnUrl)))
            .When(c => !string.IsNullOrWhiteSpace(c.ReturnUrl));
        RuleFor(c => c.LinkToken)
            .NotEmpty()
            .WithState(_ => new Error(CommonErrorCode.Required, nameof(ConfirmGoogleLinkCommand.LinkToken)));
    }

    private static string? GetAllowedOrigin(IConfiguration configuration, string app)
        => AccountSessionValidation.IsKnownApp(app)
            ? configuration[$"Authentication:Google:AllowedFrontendOrigins:{AccountSessionHandlerBase.NormalizeApp(app)}"]
            : null;
}
