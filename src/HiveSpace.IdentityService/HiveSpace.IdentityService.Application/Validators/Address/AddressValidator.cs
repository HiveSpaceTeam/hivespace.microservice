using FluentValidation;
using HiveSpace.Core.Exceptions;
using HiveSpace.Core.Exceptions.Models;
using HiveSpace.IdentityService.Application.Models.Requests;
using HiveSpace.IdentityService.Domain.Exceptions;

namespace HiveSpace.IdentityService.Application.Validators.Address;

public class AddressValidator : AbstractValidator<AddressRequestDto>
{
    public AddressValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty() 
            .WithState(_ => new ErrorCode(IdentityErrorCode.Required, nameof(AddressRequestDto.FullName)));

        RuleFor(x => x.Street)
            .NotEmpty()
            .WithState(_ => new ErrorCode(IdentityErrorCode.Required, nameof(AddressRequestDto.Street)));

        RuleFor(x => x.Ward)
            .NotEmpty()
            .WithState(_ => new ErrorCode(IdentityErrorCode.Required, nameof(AddressRequestDto.Ward)));

        RuleFor(x => x.District)
            .NotEmpty()
            .WithState(_ => new ErrorCode(IdentityErrorCode.Required, nameof(AddressRequestDto.District)));

        RuleFor(x => x.Province)
            .NotEmpty()
            .WithState(_ => new ErrorCode(IdentityErrorCode.Required, nameof(AddressRequestDto.Province)));

        RuleFor(x => x.Country)
            .NotEmpty()
            .WithState(_ => new ErrorCode(IdentityErrorCode.Required, nameof(AddressRequestDto.Country)));

        RuleFor(x => x.PhoneNumber)
            .NotEmpty()
            .WithState(_ => new ErrorCode(IdentityErrorCode.Required, nameof(AddressRequestDto.PhoneNumber)));
    }
} 