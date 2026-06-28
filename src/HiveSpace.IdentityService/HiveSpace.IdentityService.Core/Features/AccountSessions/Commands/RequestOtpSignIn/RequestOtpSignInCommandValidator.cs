using FluentValidation;
using HiveSpace.Core.Exceptions;
using HiveSpace.Core.Exceptions.Models;
using HiveSpace.IdentityService.Core.Exceptions;

namespace HiveSpace.IdentityService.Core.Features.AccountSessions.Commands.RequestOtpSignIn;

public class RequestOtpSignInCommandValidator : AbstractValidator<RequestOtpSignInCommand>
{
    public RequestOtpSignInCommandValidator()
    {
        RuleFor(c => c.Email)
            .NotEmpty()
            .WithState(_ => new Error(CommonErrorCode.Required, nameof(RequestOtpSignInCommand.Email)))
            .EmailAddress()
            .WithState(_ => new Error(IdentityDomainErrorCode.InvalidCredentials, nameof(RequestOtpSignInCommand.Email)));
    }
}
