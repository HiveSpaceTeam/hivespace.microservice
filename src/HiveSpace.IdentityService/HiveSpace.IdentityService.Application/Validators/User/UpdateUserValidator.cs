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
            .WithState(_ => new ErrorCode { Code = IdentityErrorCode.InvalidUsername, Source = nameof(UpdateUserRequestDto.UserName) });

        RuleFor(x => x.Email)
            .EmailAddress()
            .When(x => !string.IsNullOrWhiteSpace(x.Email))
            .WithState(_ => new ErrorCode { Code = IdentityErrorCode.InvalidEmail, Source = nameof(UpdateUserRequestDto.Email) });

        RuleFor(x => x.FullName)
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.FullName))
            .WithState(_ => new ErrorCode { Code = IdentityErrorCode.InvalidFullName, Source = nameof(UpdateUserRequestDto.FullName) });

        RuleFor(x => x.PhoneNumber)
            .Matches("^[+]?[1-9]\\d{1,14}$")
            .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber))
            .WithState(_ => new ErrorCode { Code = IdentityErrorCode.InvalidPhoneNumber, Source = nameof(UpdateUserRequestDto.PhoneNumber) });

        RuleFor(x => x.DateOfBirth)
            .LessThan(DateTime.Today)
            .When(x => x.DateOfBirth.HasValue)
            .WithState(_ => new ErrorCode { Code = IdentityErrorCode.InvalidDateOfBirth, Source = nameof(UpdateUserRequestDto.DateOfBirth) });
    }
} 