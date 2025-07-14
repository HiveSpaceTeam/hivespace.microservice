using FluentValidation;
using HiveSpace.Core.Exceptions;
using HiveSpace.Core.Exceptions.Models;
using HiveSpace.IdentityService.Application.Models.Requests;
using HiveSpace.IdentityService.Domain.Exceptions;

namespace HiveSpace.IdentityService.Application.Validators.User;

public class UpdateUserValidator : AbstractValidator<UpdateUserRequestDto>
{
    public UpdateUserValidator()
    {
        RuleFor(x => x.UserName)
            .MinimumLength(3)
            .MaximumLength(50)
            .Matches("^[a-zA-Z0-9_]+$")
            .When(x => !string.IsNullOrWhiteSpace(x.UserName))
            .WithState(_ => new ErrorCode(IdentityErrorCode.InvalidUsername.Code, IdentityErrorCode.InvalidUsername.Name, nameof(UpdateUserRequestDto.UserName)));

        RuleFor(x => x.Email)
            .EmailAddress()
            .When(x => !string.IsNullOrWhiteSpace(x.Email))
            .WithState(_ => new ErrorCode(IdentityErrorCode.InvalidEmail.Code, IdentityErrorCode.InvalidEmail.Name, nameof(UpdateUserRequestDto.Email)));

        RuleFor(x => x.FullName)
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.FullName))
            .WithState(_ => new ErrorCode(IdentityErrorCode.InvalidFullName.Code, IdentityErrorCode.InvalidFullName.Name, nameof(UpdateUserRequestDto.FullName)));

        RuleFor(x => x.PhoneNumber)
            .Matches("^[+]?[1-9]\\d{1,14}$")
            .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber))
            .WithState(_ => new ErrorCode(IdentityErrorCode.InvalidPhoneNumber.Code, IdentityErrorCode.InvalidPhoneNumber.Name, nameof(UpdateUserRequestDto.PhoneNumber)));

        RuleFor(x => x.DateOfBirth)
            .LessThan(DateTime.Today)
            .When(x => x.DateOfBirth.HasValue)
            .WithState(_ => new ErrorCode(IdentityErrorCode.InvalidDateOfBirth.Code, IdentityErrorCode.InvalidDateOfBirth.Name, nameof(UpdateUserRequestDto.DateOfBirth)));
    }
} 