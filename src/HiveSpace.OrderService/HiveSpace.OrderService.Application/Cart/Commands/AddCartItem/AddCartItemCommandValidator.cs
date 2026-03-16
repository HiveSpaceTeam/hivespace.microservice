using FluentValidation;
using HiveSpace.Core.Exceptions.Models;
using HiveSpace.OrderService.Domain.Exceptions;

namespace HiveSpace.OrderService.Application.Cart.Commands.AddCartItem;

public class AddCartItemCommandValidator : AbstractValidator<AddCartItemCommand>
{
    public AddCartItemCommandValidator()
    {
        RuleFor(x => x.ProductId)
            .GreaterThan(0)
            .WithState(_ => new Error(OrderDomainErrorCode.CartProductIdRequired, nameof(AddCartItemCommand.ProductId)));

        RuleFor(x => x.SkuId)
            .GreaterThan(0)
            .WithState(_ => new Error(OrderDomainErrorCode.CartSkuIdRequired, nameof(AddCartItemCommand.SkuId)));

        RuleFor(x => x.Quantity)
            .GreaterThan(0)
            .WithState(_ => new Error(OrderDomainErrorCode.CartInvalidQuantity, nameof(AddCartItemCommand.Quantity)));
    }
}
