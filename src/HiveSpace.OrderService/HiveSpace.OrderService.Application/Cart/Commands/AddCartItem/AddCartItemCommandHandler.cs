using HiveSpace.Application.Shared.Handlers;
using HiveSpace.Core.Contexts;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.OrderService.Domain.Exceptions;
using HiveSpace.OrderService.Domain.External;
using HiveSpace.OrderService.Domain.Repositories;

namespace HiveSpace.OrderService.Application.Cart.Commands.AddCartItem;

public class AddCartItemCommandHandler(
    ICartRepository cartRepository,
    ISkuRefRepository skuRefRepository,
    IPlatformCurrencyPolicyRefRepository currencyPolicyRepository,
    IUserContext userContext)
    : ICommandHandler<AddCartItemCommand, Guid>
{
    public async Task<Guid> Handle(AddCartItemCommand request, CancellationToken cancellationToken)
    {
        var sku = await skuRefRepository.GetByIdAsync(request.SkuId, request.ProductId, cancellationToken);
        if (sku is null)
            throw new NotFoundException(OrderDomainErrorCode.CartSkuNotBelongToProduct, nameof(SkuRef));

        var currencyPolicy = await currencyPolicyRepository.GetCurrentAsync(cancellationToken)
            ?? throw new InvalidFieldException(OrderDomainErrorCode.PlatformCurrencyPolicyMissing, nameof(currencyPolicyRepository));

        if (!currencyPolicy.IsCurrencyEnabled(sku.Currency))
            throw new InvalidFieldException(OrderDomainErrorCode.PlatformCurrencyDisabled, nameof(request.SkuId));

        var userId = userContext.UserId;
        var cart = await cartRepository.GetByUserIdAsync(userId, cancellationToken);

        if (cart is null)
        {
            cart = Domain.Aggregates.Carts.Cart.Create(userId);
            cartRepository.Add(cart);
        }
        else
        {
            var existingSkuIds = cart.Items.Select(i => i.SkuId).Distinct().ToList();
            if (existingSkuIds.Count > 0)
            {
                var existingSkus = await skuRefRepository.GetByIdsAsync(existingSkuIds, cancellationToken);
                var existingCurrencies = existingSkus
                    .Select(x => x.Currency)
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                if (existingCurrencies.Count > 1 ||
                    (existingCurrencies.Count == 1 &&
                     !existingCurrencies[0].Equals(sku.Currency, StringComparison.OrdinalIgnoreCase)))
                {
                    throw new InvalidFieldException(OrderDomainErrorCode.CheckoutMixedCurrencyNotAllowed, nameof(request.SkuId));
                }
            }
        }

        cart.AddItem(request.ProductId, request.SkuId, request.Quantity);
        await cartRepository.SaveChangesAsync(cancellationToken);

        return cart.Items.First(i => i.ProductId == request.ProductId && i.SkuId == request.SkuId).Id;
    }
}
