using FluentValidation;
using HiveSpace.Core.Exceptions;
using HiveSpace.Core.Exceptions.Models;
using HiveSpace.IdentityService.Core.Exceptions;
using HiveSpace.IdentityService.Core.Features.AccountSessions.Commands;

namespace HiveSpace.IdentityService.Core.Features.AccountSessions.Commands.SignIn;

public class SignInCommandValidator : AbstractValidator<SignInCommand>
{
    public SignInCommandValidator()
    {
        RuleFor(c => c.Email)
            .NotEmpty()
            .WithState(_ => new Error(CommonErrorCode.Required, nameof(SignInCommand.Email)))
            .EmailAddress()
            .WithState(_ => new Error(IdentityDomainErrorCode.InvalidCredentials, nameof(SignInCommand.Email)));
        RuleFor(c => c.Password)
            .NotEmpty()
            .WithState(_ => new Error(CommonErrorCode.Required, nameof(SignInCommand.Password)));
        RuleFor(c => c.App)
            .Must(AccountSessionValidation.IsKnownApp)
            .WithState(_ => new Error(CommonErrorCode.InvalidArgument, nameof(SignInCommand.App)));
        RuleFor(c => c.ReturnUrl).Must(AccountSessionValidation.IsSafeReturnUrl)
            .WithState(_ => new Error(IdentityDomainErrorCode.InvalidReturnUrl, nameof(SignInCommand.ReturnUrl)))
            .When(c => !string.IsNullOrWhiteSpace(c.ReturnUrl));
    }
}
