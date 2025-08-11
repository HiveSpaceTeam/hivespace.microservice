using FluentValidation;
using HiveSpace.OrderService.Application.DTOs;

namespace HiveSpace.OrderService.Application.Validators;

public class CreateOrderRequestValidator : AbstractValidator<CreateOrderRequest>
{
    public CreateOrderRequestValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty()
            .WithMessage("CustomerId is required");

        RuleFor(x => x.ShippingFee)
            .GreaterThanOrEqualTo(0)
            .WithMessage("ShippingFee must be greater than or equal to 0");

        RuleFor(x => x.Discount)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Discount must be greater than or equal to 0");

        RuleFor(x => x.PaymentMethod)
            .IsInEnum()
            .WithMessage("PaymentMethod is invalid");

        RuleFor(x => x.ShippingAddress)
            .NotNull()
            .WithMessage("ShippingAddress is required")
            .SetValidator(new ShippingAddressDtoValidator());

        RuleFor(x => x.Items)
            .NotEmpty()
            .WithMessage("Order must have at least one item");

        RuleForEach(x => x.Items)
            .SetValidator(new OrderItemBaseDtoValidator());
    }
}