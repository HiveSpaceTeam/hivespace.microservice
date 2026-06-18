using FluentValidation;
using HiveSpace.Core.Exceptions;
using HiveSpace.Core.Exceptions.Models;
using HiveSpace.UserService.Application.UserAddresses.Dtos;
using HiveSpace.UserService.Domain.Exceptions;

namespace HiveSpace.UserService.Application.UserAddresses.Commands.UpdateUserAddress;

public class UpdateUserAddressCommandValidator : AbstractValidator<UpdateUserAddressCommand>
{
    public UpdateUserAddressCommandValidator()
    {
        RuleFor(x => x.Payload.FullName)
            .NotEmpty()
            .WithState(_ => new Error(CommonErrorCode.Required, nameof(UserAddressRequestDto.FullName)))
            .MaximumLength(100)
            .WithState(_ => new Error(UserDomainErrorCode.InvalidAddress, nameof(UserAddressRequestDto.FullName)));

        RuleFor(x => x.Payload.PhoneNumber)
            .NotEmpty()
            .WithState(_ => new Error(CommonErrorCode.Required, nameof(UserAddressRequestDto.PhoneNumber)))
            .MaximumLength(20)
            .WithState(_ => new Error(UserDomainErrorCode.InvalidPhoneNumber, nameof(UserAddressRequestDto.PhoneNumber)));

        RuleFor(x => x.Payload.Street)
            .NotEmpty()
            .WithState(_ => new Error(CommonErrorCode.Required, nameof(UserAddressRequestDto.Street)))
            .MaximumLength(200)
            .WithState(_ => new Error(UserDomainErrorCode.InvalidAddress, nameof(UserAddressRequestDto.Street)));

        RuleFor(x => x.Payload.Commune)
            .NotEmpty()
            .WithState(_ => new Error(CommonErrorCode.Required, nameof(UserAddressRequestDto.Commune)))
            .MaximumLength(100)
            .WithState(_ => new Error(UserDomainErrorCode.InvalidAddress, nameof(UserAddressRequestDto.Commune)));

        RuleFor(x => x.Payload.Province)
            .NotEmpty()
            .WithState(_ => new Error(CommonErrorCode.Required, nameof(UserAddressRequestDto.Province)))
            .MaximumLength(100)
            .WithState(_ => new Error(UserDomainErrorCode.InvalidAddress, nameof(UserAddressRequestDto.Province)));

        RuleFor(x => x.Payload.Country)
            .NotEmpty()
            .WithState(_ => new Error(CommonErrorCode.Required, nameof(UserAddressRequestDto.Country)))
            .MaximumLength(100)
            .WithState(_ => new Error(UserDomainErrorCode.InvalidAddress, nameof(UserAddressRequestDto.Country)));

        RuleFor(x => x.Payload.AddressType)
            .IsInEnum()
            .WithState(_ => new Error(UserDomainErrorCode.InvalidAddress, nameof(UserAddressRequestDto.AddressType)));
    }
}
