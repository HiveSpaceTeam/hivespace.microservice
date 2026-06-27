using FluentValidation;
using HiveSpace.Core.Exceptions;
using HiveSpace.Core.Exceptions.Models;
using HiveSpace.IdentityService.Core.Exceptions;
using HiveSpace.IdentityService.Core.Features.AccountSessions.Commands;

namespace HiveSpace.IdentityService.Core.Features.AccountSessions.Commands.VerifyOtpSignIn;

public class VerifyOtpSignInCommandValidator : AbstractValidator<VerifyOtpSignInCommand>
{
    public VerifyOtpSignInCommandValidator()
    {
        RuleFor(c => c.ChallengeToken)
            .NotEmpty()
            .WithState(_ => new Error(CommonErrorCode.Required, nameof(VerifyOtpSignInCommand.ChallengeToken)));

        RuleFor(c => c.Code)
            .NotEmpty()
            .WithState(_ => new Error(CommonErrorCode.Required, nameof(VerifyOtpSignInCommand.Code)))
            .Length(6)
            .WithState(_ => new Error(CommonErrorCode.InvalidArgument, nameof(VerifyOtpSignInCommand.Code)))
            .Matches(@"^\d{6}$")
            .WithState(_ => new Error(CommonErrorCode.InvalidArgument, nameof(VerifyOtpSignInCommand.Code)));

        RuleFor(c => c.App)
            .Must(AccountSessionValidation.IsKnownApp)
            .WithState(_ => new Error(CommonErrorCode.InvalidArgument, nameof(VerifyOtpSignInCommand.App)));

        RuleFor(c => c.ReturnUrl)
            .Must(AccountSessionValidation.IsSafeReturnUrl)
            .WithState(_ => new Error(IdentityDomainErrorCode.InvalidReturnUrl, nameof(VerifyOtpSignInCommand.ReturnUrl)))
            .When(c => !string.IsNullOrWhiteSpace(c.ReturnUrl));
    }
}
