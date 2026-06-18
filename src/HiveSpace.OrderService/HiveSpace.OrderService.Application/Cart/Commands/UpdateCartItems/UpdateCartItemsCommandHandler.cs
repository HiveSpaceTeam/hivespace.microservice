using HiveSpace.Application.Shared.Handlers;
using HiveSpace.Core.Contexts;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.OrderService.Application.Cart;
using HiveSpace.OrderService.Domain.Aggregates.Carts;
using HiveSpace.OrderService.Domain.Exceptions;
using HiveSpace.OrderService.Domain.External;
using HiveSpace.OrderService.Domain.Repositories;

namespace HiveSpace.OrderService.Application.Cart.Commands.UpdateCartItems;

public class UpdateCartItemsCommandHandler(
    ICartRepository cartRepository,
    ICouponRepository couponRepository,
    IProductRefRepository productRefRepository,
    ISkuRefRepository skuRefRepository,
    IUserContext userContext)
    : ICommandHandler<UpdateCartItemsCommand>
{
    public async Task Handle(UpdateCartItemsCommand request, CancellationToken cancellationToken)
    {
        var userId = userContext.UserId;
        var cart = await cartRepository.GetByUserIdAsync(userId, cancellationToken)
            ?? throw new NotFoundException(OrderDomainErrorCode.CartNotFound, nameof(Cart));

        if (request.SelectAll.HasValue)
            cart.SelectAllItems(request.SelectAll.Value);

        foreach (var itemRequest in request.Items)
        {
            if (itemRequest.SkuId.HasValue)
            {
                var cartItem = cart.Items.FirstOrDefault(i => i.Id == itemRequest.CartItemId)
                    ?? throw new NotFoundException(OrderDomainErrorCode.CartItemNotFound, nameof(CartItem));

                var skuExists = await skuRefRepository.ExistsAsync(itemRequest.SkuId.Value, cartItem.ProductId, cancellationToken);
                if (!skuExists)
                    throw new NotFoundException(OrderDomainErrorCode.CartSkuNotBelongToProduct, nameof(SkuRef));
            }

            cart.UpdateItemById(itemRequest.CartItemId, itemRequest.SkuId, itemRequest.Quantity, itemRequest.IsSelected);
        }

        if (cart.AppliedStoreCoupons.Count > 0)
        {
            var selectedItems = cart.Items
                .Where(x => x.IsSelected)
                .ToList();

            var productIds = selectedItems
                .Select(x => x.ProductId)
                .Distinct()
                .ToList();
            var skuIds = selectedItems
                .Select(x => x.SkuId)
                .Distinct()
                .ToList();

            List<ProductRef> productRefs = productIds.Count > 0
                ? await productRefRepository.GetByIdsAsync(productIds, cancellationToken) : [];
            var productsById = productRefs.ToDictionary(x => x.Id);
            List<SkuRef> skuRefs = skuIds.Count > 0
                ? await skuRefRepository.GetByIdsAsync(skuIds, cancellationToken) : [];
            var skusById = skuRefs.ToDictionary(x => x.Id);

            var snapshots = SelectedCartCouponEvaluator.BuildStoreSnapshots(
                cart.Items,
                productsById,
                skusById);

            await PersistedCartCouponState.RemoveInvalidStoreCouponsAsync(
                cart,
                snapshots,
                couponRepository,
                userId,
                cancellationToken);
        }

        await cartRepository.SaveChangesAsync(cancellationToken);
    }
}
