using FluentValidation;
using HiveSpace.Core.Exceptions;
using HiveSpace.Core.Exceptions.Models;
using HiveSpace.IdentityService.Application.Models.Requests;
using HiveSpace.IdentityService.Domain.Exceptions;

namespace HiveSpace.IdentityService.Application.Validators.User;

public class ChangePasswordValidator : AbstractValidator<ChangePasswordRequestDto>
{
    public ChangePasswordValidator()
    {
        RuleFor(x => x.Password)
            .NotEmpty()
            .WithState(_ => new Error(IdentityErrorCode.Required, nameof(ChangePasswordRequestDto.Password)));

        RuleFor(x => x.NewPassword)
            .NotEmpty()
            .WithState(_ => new Error(IdentityErrorCode.Required, nameof(ChangePasswordRequestDto.NewPassword)))
            .MinimumLength(8)
            .Matches("^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d)(?=.*[@$!%*?&])[A-Za-z\\d@$!%*?&]")
            .WithState(_ => new Error(IdentityErrorCode.InvalidPasswordFormat, nameof(ChangePasswordRequestDto.NewPassword)));
    }
} 