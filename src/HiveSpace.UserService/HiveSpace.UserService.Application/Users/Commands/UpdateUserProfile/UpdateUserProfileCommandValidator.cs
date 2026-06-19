using FluentValidation;
using HiveSpace.Core.Exceptions;
using HiveSpace.Core.Exceptions.Models;
using HiveSpace.UserService.Application.Users.Dtos;
using HiveSpace.UserService.Domain.Enums;
using HiveSpace.UserService.Domain.Exceptions;

namespace HiveSpace.UserService.Application.Users.Commands.UpdateUserProfile;

public class UpdateUserProfileCommandValidator : AbstractValidator<UpdateUserProfileCommand>
{
    public UpdateUserProfileCommandValidator()
    {
        When(x => x.Payload.FullName != null, () =>
        {
            RuleFor(x => x.Payload.FullName!)
                .NotEmpty()
                .WithState(_ => new Error(CommonErrorCode.Required, nameof(UpdateUserProfileRequestDto.FullName)))
                .MinimumLength(2)
                .WithState(_ => new Error(UserDomainErrorCode.InvalidUserInformation, nameof(UpdateUserProfileRequestDto.FullName)))
                .MaximumLength(100)
                .WithState(_ => new Error(UserDomainErrorCode.InvalidUserInformation, nameof(UpdateUserProfileRequestDto.FullName)));
        });

        When(x => x.Payload.UserName != null, () =>
        {
            RuleFor(x => x.Payload.UserName!)
                .NotEmpty()
                .WithState(_ => new Error(CommonErrorCode.Required, nameof(UpdateUserProfileRequestDto.UserName)))
                .MinimumLength(3)
                .WithState(_ => new Error(UserDomainErrorCode.InvalidUserInformation, nameof(UpdateUserProfileRequestDto.UserName)))
                .MaximumLength(50)
                .WithState(_ => new Error(UserDomainErrorCode.InvalidUserInformation, nameof(UpdateUserProfileRequestDto.UserName)))
                .Matches(@"^[a-zA-Z0-9_\-@.]+$")
                .WithState(_ => new Error(UserDomainErrorCode.InvalidUserInformation, nameof(UpdateUserProfileRequestDto.UserName)));
        });

        When(x => x.Payload.PhoneNumber != null, () =>
        {
            RuleFor(x => x.Payload.PhoneNumber!)
                .NotEmpty()
                .WithState(_ => new Error(CommonErrorCode.Required, nameof(UpdateUserProfileRequestDto.PhoneNumber)));
        });

        When(x => x.Payload.Gender.HasValue, () =>
        {
            RuleFor(x => x.Payload.Gender!.Value)
                .IsInEnum()
                .WithState(_ => new Error(CommonErrorCode.InvalidArgument, nameof(UpdateUserProfileRequestDto.Gender)));
        });

        When(x => x.Payload.DateOfBirth.HasValue, () =>
        {
            RuleFor(x => x.Payload.DateOfBirth!.Value)
                .LessThan(DateTimeOffset.UtcNow)
                .WithState(_ => new Error(UserDomainErrorCode.InvalidDateOfBirth, nameof(UpdateUserProfileRequestDto.DateOfBirth)))
                .GreaterThan(DateTimeOffset.UtcNow.AddYears(-120))
                .WithState(_ => new Error(UserDomainErrorCode.InvalidDateOfBirth, nameof(UpdateUserProfileRequestDto.DateOfBirth)));
        });

        When(x => x.Payload.AvatarFileId != null, () =>
        {
            RuleFor(x => x.Payload.AvatarFileId!)
                .NotEmpty()
                .WithState(_ => new Error(CommonErrorCode.Required, nameof(UpdateUserProfileRequestDto.AvatarFileId)))
                .MaximumLength(100)
                .WithState(_ => new Error(UserDomainErrorCode.InvalidField, nameof(UpdateUserProfileRequestDto.AvatarFileId)))
                .Must(value => Guid.TryParse(value, out _))
                .WithState(_ => new Error(UserDomainErrorCode.InvalidField, nameof(UpdateUserProfileRequestDto.AvatarFileId)));
        });
    }
}
