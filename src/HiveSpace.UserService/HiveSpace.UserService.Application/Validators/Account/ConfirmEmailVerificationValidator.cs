using FluentValidation;
using HiveSpace.Core.Exceptions;
using HiveSpace.Core.Exceptions.Models;
using HiveSpace.UserService.Application.Models.Requests.Account;

namespace HiveSpace.UserService.Application.Validators.Account;

public class ConfirmEmailVerificationValidator : AbstractValidator<ConfirmEmailVerificationRequestDto>
{
    public ConfirmEmailVerificationValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty()
            .WithState(_ => new Error(CommonErrorCode.Required, nameof(ConfirmEmailVerificationRequestDto.Token)));
    }
}