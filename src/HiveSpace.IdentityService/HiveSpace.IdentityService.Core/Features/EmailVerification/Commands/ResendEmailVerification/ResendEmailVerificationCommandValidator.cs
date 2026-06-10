using FluentValidation;
using HiveSpace.Core.Exceptions;
using HiveSpace.Core.Exceptions.Models;
using HiveSpace.IdentityService.Core.Exceptions;
using HiveSpace.IdentityService.Core.Features.AccountSessions.Commands;

namespace HiveSpace.IdentityService.Core.Features.EmailVerification.Commands.ResendEmailVerification;

public class ResendEmailVerificationCommandValidator : AbstractValidator<ResendEmailVerificationCommand>
{
    public ResendEmailVerificationCommandValidator()
    {
        RuleFor(c => c.Email)
            .NotEmpty()
            .WithState(_ => new Error(CommonErrorCode.Required, nameof(ResendEmailVerificationCommand.Email)))
            .EmailAddress()
            .WithState(_ => new Error(IdentityDomainErrorCode.InvalidCredentials, nameof(ResendEmailVerificationCommand.Email)));

        RuleFor(c => c.App)
            .Must(AccountSessionValidation.IsKnownApp)
            .WithState(_ => new Error(CommonErrorCode.InvalidArgument, nameof(ResendEmailVerificationCommand.App)));

        RuleFor(c => c.ReturnUrl).Must(AccountSessionValidation.IsSafeReturnUrl)
            .WithState(_ => new Error(IdentityDomainErrorCode.InvalidReturnUrl, nameof(ResendEmailVerificationCommand.ReturnUrl)))
            .When(c => !string.IsNullOrWhiteSpace(c.ReturnUrl));
    }
}
