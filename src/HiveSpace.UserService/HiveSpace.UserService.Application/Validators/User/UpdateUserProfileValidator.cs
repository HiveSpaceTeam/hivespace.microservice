using FluentValidation;
using HiveSpace.Core.Exceptions;
using HiveSpace.Core.Exceptions.Models;
using HiveSpace.UserService.Application.DTOs.User;
using HiveSpace.UserService.Domain.Enums;
using HiveSpace.UserService.Domain.Exceptions;

namespace HiveSpace.UserService.Application.Validators.User;

public class UpdateUserProfileValidator : AbstractValidator<UpdateUserProfileRequestDto>
{
    public UpdateUserProfileValidator()
    {
        When(x => x.FullName != null, () =>
        {
            RuleFor(x => x.FullName!)
                .NotEmpty()
                .WithState(_ => new Error(CommonErrorCode.Required, nameof(UpdateUserProfileRequestDto.FullName)))
                .MinimumLength(2)
                .WithState(_ => new Error(UserDomainErrorCode.InvalidUserInformation, nameof(UpdateUserProfileRequestDto.FullName)))
                .MaximumLength(100)
                .WithState(_ => new Error(UserDomainErrorCode.InvalidUserInformation, nameof(UpdateUserProfileRequestDto.FullName)));
        });

        When(x => x.UserName != null, () =>
        {
            RuleFor(x => x.UserName!)
                .NotEmpty()
                .WithState(_ => new Error(CommonErrorCode.Required, nameof(UpdateUserProfileRequestDto.UserName)))
                .MinimumLength(3)
                .WithState(_ => new Error(UserDomainErrorCode.InvalidUserInformation, nameof(UpdateUserProfileRequestDto.UserName)))
                .MaximumLength(50)
                .WithState(_ => new Error(UserDomainErrorCode.InvalidUserInformation, nameof(UpdateUserProfileRequestDto.UserName)))
                .Matches(@"^[a-zA-Z0-9_\-@.]+$")
                .WithState(_ => new Error(UserDomainErrorCode.InvalidUserInformation, nameof(UpdateUserProfileRequestDto.UserName)));
        });

        When(x => x.PhoneNumber != null, () =>
        {
            RuleFor(x => x.PhoneNumber!)
                .NotEmpty()
                .WithState(_ => new Error(CommonErrorCode.Required, nameof(UpdateUserProfileRequestDto.PhoneNumber)));
        });

        When(x => x.Gender.HasValue, () =>
        {
            RuleFor(x => x.Gender!.Value)
                .IsInEnum()
                .WithState(_ => new Error(CommonErrorCode.InvalidArgument, nameof(UpdateUserProfileRequestDto.Gender)));
        });

        When(x => x.DateOfBirth.HasValue, () =>
        {
            RuleFor(x => x.DateOfBirth!.Value)
                .LessThan(DateTimeOffset.UtcNow)
                .WithState(_ => new Error(UserDomainErrorCode.InvalidDateOfBirth, nameof(UpdateUserProfileRequestDto.DateOfBirth)))
                .GreaterThan(DateTimeOffset.UtcNow.AddYears(-120))
                .WithState(_ => new Error(UserDomainErrorCode.InvalidDateOfBirth, nameof(UpdateUserProfileRequestDto.DateOfBirth)));
        });
    }
}
