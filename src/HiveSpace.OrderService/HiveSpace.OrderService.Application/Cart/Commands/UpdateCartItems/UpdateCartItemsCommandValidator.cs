using FluentValidation;
using HiveSpace.Core.Exceptions.Models;
using HiveSpace.OrderService.Domain.Exceptions;

namespace HiveSpace.OrderService.Application.Cart.Commands.UpdateCartItems;

public class UpdateCartItemsCommandValidator : AbstractValidator<UpdateCartItemsCommand>
{
    public UpdateCartItemsCommandValidator()
    {
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(x => x.CartItemId)
                .NotEmpty()
                .WithState(_ => new Error(OrderDomainErrorCode.CartItemNotFound, nameof(CartItemUpdateRequest.CartItemId)));

            item.RuleFor(x => x.SkuId)
                .GreaterThan(0)
                .When(x => x.SkuId.HasValue)
                .WithState(_ => new Error(OrderDomainErrorCode.CartSkuIdRequired, nameof(CartItemUpdateRequest.SkuId)));

            item.RuleFor(x => x.Quantity)
                .GreaterThan(0)
                .When(x => x.Quantity.HasValue)
                .WithState(_ => new Error(OrderDomainErrorCode.CartInvalidQuantity, nameof(CartItemUpdateRequest.Quantity)));
        });
    }
}
