using HiveSpace.Domain.Shared.ValueObjects;
using HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.Commands;
using HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.Dtos;
using HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.Events;
using HiveSpace.OrderService.Domain.Repositories;
using MassTransit;
using Microsoft.Extensions.Logging;
using static HiveSpace.OrderService.Application.Cart.CheckoutCalculator;

namespace HiveSpace.OrderService.Api.Consumers.Saga.CheckoutSaga;

public class ValidateCheckoutConsumer(
    ICartRepository cartRepository,
    ICouponRepository couponRepository,
    IProductRefRepository productRefRepository,
    ISkuRefRepository skuRefRepository,
    ILogger<ValidateCheckoutConsumer> logger) : IConsumer<ValidateCheckout>
{
    public async Task Consume(ConsumeContext<ValidateCheckout> context)
    {
        var message = context.Message;
        var ct = context.CancellationToken;

        var cart = await cartRepository.GetByUserIdAsync(message.UserId, ct);
        if (cart is null || cart.Items.Count == 0)
        {
            await context.RespondAsync<ValidationFailed>(new
            {
                message.CorrelationId,
                Reason = "Cart is empty or not found",
                Errors = new List<string> { "Cart not found for user" }
            });
            return;
        }

        var selectedItems = cart.Items.Where(i => i.IsSelected).ToList();
        if (selectedItems.Count == 0)
        {
            await context.RespondAsync<ValidationFailed>(new
            {
                message.CorrelationId,
                Reason = "No items selected in cart",
                Errors = new List<string> { "No items selected" }
            });
            return;
        }

        var productRefIds = selectedItems.Select(i => i.ProductId).Distinct().ToList();
        var productRefs   = await productRefRepository.GetByIdsAsync(productRefIds, ct);
        var productRefMap = productRefs.ToDictionary(p => p.Id);

        var skuRefIds = selectedItems.Select(i => i.SkuId).Distinct().ToList();
        var skuRefs   = await skuRefRepository.GetByIdsAsync(skuRefIds, ct);
        var skuRefMap = skuRefs.ToDictionary(s => s.Id);

        var missingProductIds = productRefIds.Except(productRefMap.Keys).ToList();
        if (missingProductIds.Count > 0)
        {
            await context.RespondAsync<ValidationFailed>(new
            {
                message.CorrelationId,
                Reason = $"Products not found: {string.Join(", ", missingProductIds)}",
                Errors = missingProductIds.Select(id => $"Product {id} not found").ToList()
            });
            return;
        }

        var missingSkuIds = skuRefIds.Except(skuRefMap.Keys).ToList();
        if (missingSkuIds.Count > 0)
        {
            await context.RespondAsync<ValidationFailed>(new
            {
                message.CorrelationId,
                Reason = $"SKUs not found: {string.Join(", ", missingSkuIds)}",
                Errors = missingSkuIds.Select(id => $"SKU {id} not found").ToList()
            });
            return;
        }

        var items = selectedItems.Select(i =>
        {
            var productRef = productRefMap[i.ProductId];
            var skuRef     = skuRefMap[i.SkuId];
            return new OrderItemDto
            {
                ProductId   = i.ProductId,
                SkuId       = i.SkuId,
                StoreId     = productRef.StoreId,
                Quantity    = i.Quantity,
                Price       = skuRef.Price,
                ProductName = productRef.Name,
                SkuName     = string.Empty,
                ImageUrl    = skuRef.ImageUrl ?? productRef.ThumbnailUrl ?? string.Empty
            };
        }).ToList();

        var subtotal       = items.Sum(i => i.Price * i.Quantity);
        var totalItemCount = items.Sum(i => i.Quantity);
        var shippingFee    = CalculateShippingFee(totalItemCount);
        var discountAmount = 0L;

        if (message.CouponCodes.Count > 0)
        {
            var coupons    = await couponRepository.GetByCodesAsync(message.CouponCodes, ct);
            var productIds = items.Select(i => i.ProductId).Distinct().ToList();
            var storeIds   = items.Select(i => i.StoreId).Distinct().ToList();
            var storeId    = storeIds.Count == 1 ? storeIds[0] : (Guid?)null;

            foreach (var coupon in coupons)
            {
                var (itemDiscount, _) = ApplyCoupon(coupon, message.UserId, subtotal, shippingFee, productIds, storeId);
                if (itemDiscount > 0)
                    discountAmount += itemDiscount;
                else
                    logger.LogWarning("Coupon {Code} invalid for user {UserId}", coupon.Code, message.UserId);
            }
        }

        var grandTotal = subtotal + shippingFee - discountAmount;

        await context.RespondAsync<ValidationCompleted>(new
        {
            message.CorrelationId,
            Items          = items,
            Subtotal       = subtotal,
            ShippingFee    = shippingFee,
            TaxAmount      = 0L,
            DiscountAmount = discountAmount,
            GrandTotal     = grandTotal
        });
    }
}
