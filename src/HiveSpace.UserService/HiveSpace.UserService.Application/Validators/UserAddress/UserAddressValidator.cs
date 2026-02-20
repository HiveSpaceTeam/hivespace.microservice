using FluentValidation;
using HiveSpace.UserService.Application.DTOs.UserAddress;
using HiveSpace.UserService.Domain.Exceptions;

namespace HiveSpace.UserService.Application.Validators.UserAddress;

public class UserAddressValidator : AbstractValidator<UserAddressRequestDto>
{
    public UserAddressValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Full Name is required")
            .MaximumLength(100).WithMessage("Full Name must not exceed 100 characters");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("Phone Number is required")
            .MaximumLength(20).WithMessage("Phone Number must not exceed 20 characters");

        RuleFor(x => x.Street)
            .NotEmpty().WithMessage("Street is required")
            .MaximumLength(200).WithMessage("Street must not exceed 200 characters");

        RuleFor(x => x.District)
            .NotEmpty().WithMessage("District is required")
            .MaximumLength(100).WithMessage("District must not exceed 100 characters");

        RuleFor(x => x.Province)
            .NotEmpty().WithMessage("Province is required")
            .MaximumLength(100).WithMessage("Province must not exceed 100 characters");

        RuleFor(x => x.Country)
            .NotEmpty().WithMessage("Country is required")
            .MaximumLength(100).WithMessage("Country must not exceed 100 characters");

        RuleFor(x => x.AddressType)
            .IsInEnum().WithMessage("Invalid Address Type");
    }
}
