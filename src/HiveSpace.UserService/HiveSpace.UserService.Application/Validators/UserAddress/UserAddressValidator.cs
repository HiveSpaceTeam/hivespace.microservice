using FluentValidation;
using HiveSpace.Core.Exceptions;
using HiveSpace.Core.Exceptions.Models;
using HiveSpace.Domain.Shared.Errors;
using HiveSpace.UserService.Application.DTOs.UserAddress;
using HiveSpace.UserService.Domain.Exceptions;

namespace HiveSpace.UserService.Application.Validators.UserAddress;

public class UserAddressValidator : AbstractValidator<UserAddressRequestDto>
{
    public UserAddressValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty()
            .WithState(_ => new Error(CommonErrorCode.Required, nameof(UserAddressRequestDto.FullName)))
            .MaximumLength(100)
            .WithState(_ => new Error(UserDomainErrorCode.InvalidAddress, nameof(UserAddressRequestDto.FullName)));

        RuleFor(x => x.PhoneNumber)
            .NotEmpty()
            .WithState(_ => new Error(CommonErrorCode.Required, nameof(UserAddressRequestDto.PhoneNumber)))
            .MaximumLength(20)
            .WithState(_ => new Error(UserDomainErrorCode.InvalidPhoneNumber, nameof(UserAddressRequestDto.PhoneNumber)));

        RuleFor(x => x.Street)
            .NotEmpty()
            .WithState(_ => new Error(CommonErrorCode.Required, nameof(UserAddressRequestDto.Street)))
            .MaximumLength(200)
            .WithState(_ => new Error(UserDomainErrorCode.InvalidAddress, nameof(UserAddressRequestDto.Street)));

        RuleFor(x => x.District)
            .NotEmpty()
            .WithState(_ => new Error(CommonErrorCode.Required, nameof(UserAddressRequestDto.District)))
            .MaximumLength(100)
            .WithState(_ => new Error(UserDomainErrorCode.InvalidAddress, nameof(UserAddressRequestDto.District)));

        RuleFor(x => x.Province)
            .NotEmpty()
            .WithState(_ => new Error(CommonErrorCode.Required, nameof(UserAddressRequestDto.Province)))
            .MaximumLength(100)
            .WithState(_ => new Error(UserDomainErrorCode.InvalidAddress, nameof(UserAddressRequestDto.Province)));

        RuleFor(x => x.Country)
            .NotEmpty()
            .WithState(_ => new Error(CommonErrorCode.Required, nameof(UserAddressRequestDto.Country)))
            .MaximumLength(100)
            .WithState(_ => new Error(UserDomainErrorCode.InvalidAddress, nameof(UserAddressRequestDto.Country)));

        RuleFor(x => x.AddressType)
            .IsInEnum()
            .WithState(_ => new Error(UserDomainErrorCode.InvalidAddress, nameof(UserAddressRequestDto.AddressType)));
    }
}
