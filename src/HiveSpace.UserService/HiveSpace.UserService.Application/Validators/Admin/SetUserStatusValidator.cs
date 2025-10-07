using FluentValidation;
using HiveSpace.Core.Exceptions;
using HiveSpace.Core.Exceptions.Models;
using HiveSpace.UserService.Application.Models.Requests.Admin;
using HiveSpace.UserService.Application.Constant.Enum;

namespace HiveSpace.UserService.Application.Validators.Admin;

public class SetUserStatusValidator : AbstractValidator<SetUserStatusRequestDto>
{
    public SetUserStatusValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithState(_ => new Error(CommonErrorCode.Required, nameof(SetUserStatusRequestDto.UserId)));

        RuleFor(x => x.ResponseType)
            .IsInEnum()
            .WithState(_ => new Error(CommonErrorCode.Required, nameof(SetUserStatusRequestDto.ResponseType)));
    }
}