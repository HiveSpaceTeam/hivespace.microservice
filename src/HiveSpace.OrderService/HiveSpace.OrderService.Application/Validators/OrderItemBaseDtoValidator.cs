using FluentValidation;
using HiveSpace.OrderService.Application.DTOs;

namespace HiveSpace.OrderService.Application.Validators;

public class OrderItemBaseDtoValidator : AbstractValidator<OrderItemBaseDto>
{
    public OrderItemBaseDtoValidator()
    {
        RuleFor(x => x.SkuId)
            .GreaterThan(0)
            .WithMessage("SkuId must be greater than 0");

        RuleFor(x => x.ProductName)
            .NotEmpty()
            .WithMessage("ProductName is required")
            .MaximumLength(200)
            .WithMessage("ProductName must not exceed 200 characters");

        RuleFor(x => x.VariantName)
            .NotEmpty()
            .WithMessage("VariantName is required")
            .MaximumLength(200)
            .WithMessage("VariantName must not exceed 200 characters");

        RuleFor(x => x.Quantity)
            .GreaterThan(0)
            .WithMessage("Quantity must be greater than 0");

        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("Amount must be greater than 0");

        RuleFor(x => x.Currency)
            .IsInEnum()
            .WithMessage("Currency is invalid");
    }
}