using FluentValidation;
using HiveSpace.Core.Exceptions;
using HiveSpace.Core.Exceptions.Models;
using HiveSpace.IdentityService.Core.Exceptions;
using HiveSpace.IdentityService.Core.Features.AccountSessions.Commands;

namespace HiveSpace.IdentityService.Core.Features.AccountSessions.Commands.RegisterAccount;

public class RegisterAccountCommandValidator : AbstractValidator<RegisterAccountCommand>
{
    public RegisterAccountCommandValidator()
    {
        RuleFor(c => c.Email)
            .NotEmpty()
            .WithState(_ => new Error(CommonErrorCode.Required, nameof(RegisterAccountCommand.Email)))
            .EmailAddress()
            .WithState(_ => new Error(IdentityDomainErrorCode.InvalidCredentials, nameof(RegisterAccountCommand.Email)));
        RuleFor(c => c.Password)
            .NotEmpty()
            .WithState(_ => new Error(CommonErrorCode.Required, nameof(RegisterAccountCommand.Password)));
        RuleFor(c => c.ConfirmPassword)
            .Equal(c => c.Password)
            .WithState(_ => new Error(CommonErrorCode.InvalidArgument, nameof(RegisterAccountCommand.ConfirmPassword)));
        RuleFor(c => c.App)
            .Must(AccountSessionValidation.IsKnownApp)
            .WithState(_ => new Error(CommonErrorCode.InvalidArgument, nameof(RegisterAccountCommand.App)));
        RuleFor(c => c.ReturnUrl).Must(AccountSessionValidation.IsSafeReturnUrl)
            .WithState(_ => new Error(IdentityDomainErrorCode.InvalidReturnUrl, nameof(RegisterAccountCommand.ReturnUrl)))
            .When(c => !string.IsNullOrWhiteSpace(c.ReturnUrl));
    }
}
