using FluentValidation;
using HiveSpace.Core.Exceptions;
using HiveSpace.Core.Exceptions.Models;
using HiveSpace.IdentityService.Application.Models.Requests;
using HiveSpace.IdentityService.Domain.Exceptions;

namespace HiveSpace.IdentityService.Application.Validators.User;

public class SignupValidator : AbstractValidator<SignupRequestDto>
{
    public SignupValidator()
    {
        RuleFor(x => x.UserName)
            .NotEmpty()
            .WithState(_ => new Error(IdentityErrorCode.Required, nameof(SignupRequestDto.UserName)))
            .MinimumLength(3)
            .MaximumLength(50)
            .Matches("^[a-zA-Z0-9_]+$")
            .WithState(_ => new Error(IdentityErrorCode.InvalidUsername, nameof(SignupRequestDto.UserName)));

        RuleFor(x => x.Email)
            .NotEmpty()
            .WithState(_ => new Error(IdentityErrorCode.Required, nameof(SignupRequestDto.Email)))
            .EmailAddress()
            .WithState(_ => new Error(IdentityErrorCode.InvalidEmail, nameof(SignupRequestDto.Email)));

        RuleFor(x => x.FullName)
            .NotEmpty()
            .WithState(_ => new Error(IdentityErrorCode.Required, nameof(SignupRequestDto.FullName)))
            .MaximumLength(100)
            .WithState(_ => new Error(IdentityErrorCode.InvalidFullName, nameof(SignupRequestDto.FullName)));

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithState(_ => new Error(IdentityErrorCode.Required, nameof(SignupRequestDto.Password)))
            .MinimumLength(8)
            .Matches("^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d)(?=.*[@$!%*?&])[A-Za-z\\d@$!%*?&]")
            .WithState(_ => new Error(IdentityErrorCode.InvalidPasswordFormat, nameof(SignupRequestDto.Password)));

        RuleFor(x => x.ConfirmPassword)
            .NotEmpty()
            .WithState(_ => new Error(IdentityErrorCode.Required, nameof(SignupRequestDto.ConfirmPassword)))
            .Equal(x => x.Password)
            .WithState(_ => new Error(IdentityErrorCode.PasswordMismatch, nameof(SignupRequestDto.ConfirmPassword)));
    }
} 