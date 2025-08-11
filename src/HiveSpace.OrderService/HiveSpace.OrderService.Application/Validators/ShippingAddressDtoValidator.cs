using FluentValidation;
using HiveSpace.OrderService.Application.DTOs;

namespace HiveSpace.OrderService.Application.Validators;

public class ShippingAddressDtoValidator : AbstractValidator<ShippingAddressDto>
{
    public ShippingAddressDtoValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty()
            .WithMessage("FullName is required")
            .MaximumLength(100)
            .WithMessage("FullName must not exceed 100 characters");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty()
            .WithMessage("PhoneNumber is required")
            .Matches(@"^(\+84|0)[0-9]{9,10}$")
            .WithMessage("PhoneNumber format is invalid");

        RuleFor(x => x.Street)
            .NotEmpty()
            .WithMessage("Street is required")
            .MaximumLength(200)
            .WithMessage("Street must not exceed 200 characters");

        RuleFor(x => x.Ward)
            .NotEmpty()
            .WithMessage("Ward is required")
            .MaximumLength(100)
            .WithMessage("Ward must not exceed 100 characters");

        RuleFor(x => x.District)
            .NotEmpty()
            .WithMessage("District is required")
            .MaximumLength(100)
            .WithMessage("District must not exceed 100 characters");

        RuleFor(x => x.Province)
            .NotEmpty()
            .WithMessage("Province is required")
            .MaximumLength(100)
            .WithMessage("Province must not exceed 100 characters");

        RuleFor(x => x.Country)
            .NotEmpty()
            .WithMessage("Country is required")
            .MaximumLength(100)
            .WithMessage("Country must not exceed 100 characters");
    }
}