using HiveSpace.Domain.Shared.Entities;
using HiveSpace.Domain.Shared.ValueObjects;
using HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.Commands;
using HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.Dtos;
using HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.Events;
using HiveSpace.OrderService.Domain.Aggregates.Orders;
using HiveSpace.OrderService.Domain.Enumerations;
using HiveSpace.OrderService.Domain.Repositories;
using HiveSpace.OrderService.Domain.ValueObjects;
using HiveSpace.OrderService.Domain.Aggregates.Coupons;
using MassTransit;
using Microsoft.Extensions.Logging;
using DomainPaymentMethod = HiveSpace.OrderService.Domain.Enumerations.PaymentMethod;
using MessagingPaymentMethod = HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.PaymentMethod;
using static HiveSpace.OrderService.Application.Cart.CheckoutCalculator;

namespace HiveSpace.OrderService.Api.Consumers.Saga.CheckoutSaga;

public class CreateOrderConsumer(
    IOrderRepository orderRepository,
    ICartRepository cartRepository,
    IProductRefRepository productRefRepository,
    ISkuRefRepository skuRefRepository,
    ICouponRepository couponRepository,
    ILogger<CreateOrderConsumer> logger) : IConsumer<CreateOrder>
{
    public async Task Consume(ConsumeContext<CreateOrder> context)
    {
        var message = context.Message;
        var ct = context.CancellationToken;

        // Load selected cart items
        var cart = await cartRepository.GetByUserIdAsync(message.UserId, ct);
        var selectedItems = cart?.Items.Where(i => i.IsSelected).ToList() ?? [];

        if (selectedItems.Count == 0)
        {
            await context.RespondAsync<OrderCreationFailed>(new
            {
                message.CorrelationId,
                Reason = "No selected items in cart",
                Errors = new List<string> { "Cart is empty or no items selected" }
            });
            return;
        }

        var productRefIds = selectedItems.Select(i => i.ProductId).Distinct().ToList();
        var skuRefIds     = selectedItems.Select(i => i.SkuId).Distinct().ToList();

        var productRefs = (await productRefRepository.GetByIdsAsync(productRefIds, ct)).ToDictionary(p => p.Id);
        var skuRefs     = (await skuRefRepository.GetByIdsAsync(skuRefIds, ct)).ToDictionary(s => s.Id);

        var missingProducts = productRefIds.Except(productRefs.Keys).ToList();
        var missingSkus     = skuRefIds.Except(skuRefs.Keys).ToList();

        if (missingProducts.Count > 0 || missingSkus.Count > 0)
        {
            await context.RespondAsync<OrderCreationFailed>(new
            {
                message.CorrelationId,
                Reason = "Some products or SKUs not found",
                Errors = missingProducts.Select(id => $"Product {id} not found")
                    .Concat(missingSkus.Select(id => $"SKU {id} not found"))
                    .ToList()
            });
            return;
        }

        var phone   = new PhoneNumber(message.DeliveryAddress.Phone);
        var address = new DeliveryAddress(
            message.DeliveryAddress.RecipientName,
            phone,
            message.DeliveryAddress.StreetAddress,
            message.DeliveryAddress.Commune,
            message.DeliveryAddress.Province,
            message.DeliveryAddress.Country,
            message.DeliveryAddress.Notes ?? string.Empty);

        var isCOD            = message.PaymentMethod == MessagingPaymentMethod.COD;
        var domainPayment    = Enumeration.FromDisplayName<DomainPaymentMethod>(message.PaymentMethod.ToString());
        var requestedCouponCodes = message.CouponCodes
            .Where(code => !string.IsNullOrWhiteSpace(code))
            .Select(code => code.Trim().ToUpperInvariant())
            .Distinct()
            .ToList();
        var coupons          = message.CouponCodes.Count > 0
            ? await couponRepository.GetByCodesAsync(message.CouponCodes, ct)
            : [];

        var itemsByStore     = selectedItems.GroupBy(i => productRefs[i.ProductId].StoreId).ToList();
        var checkoutStoreIds = itemsByStore.Select(g => g.Key).ToHashSet();

        if (requestedCouponCodes.Count > 0)
        {
            var returnedCouponCodes = coupons
                .Select(c => c.Code.Trim().ToUpperInvariant())
                .ToHashSet();

            var missingCouponCodes = requestedCouponCodes
                .Where(code => !returnedCouponCodes.Contains(code))
                .ToList();

            if (missingCouponCodes.Count > 0)
            {
                await context.RespondAsync<OrderCreationFailed>(new
                {
                    message.CorrelationId,
                    Reason = "Some coupons not found",
                    Errors = missingCouponCodes.Select(code => $"Coupon {code} not found").ToList()
                });
                return;
            }

            var unmappedCoupons = coupons
                .Where(c => c.OwnerType == CouponOwnerType.Store && (!c.StoreId.HasValue || !checkoutStoreIds.Contains(c.StoreId.Value)))
                .Select(c => c.Code)
                .Distinct()
                .ToList();

            if (unmappedCoupons.Count > 0)
            {
                await context.RespondAsync<OrderCreationFailed>(new
                {
                    message.CorrelationId,
                    Reason = "Some coupons are not applicable to this checkout",
                    Errors = unmappedCoupons.Select(code => $"Coupon {code} is not applicable to selected store items").ToList()
                });
                return;
            }
        }

        var platformCoupons  = coupons.Where(c => c.OwnerType == CouponOwnerType.Platform).ToList();

        var shippingPerStore = DistributeShippingFee(
            CalculateShippingFee(selectedItems.Sum(i => i.Quantity)), itemsByStore.Count);

        var createdOrders = new List<Order>();
        var allItemDtos   = new List<OrderItemDto>();

        for (int i = 0; i < itemsByStore.Count; i++)
        {
            var storeGroup  = itemsByStore[i];
            var storeId     = storeGroup.Key;
            var pkgShipping = shippingPerStore[i];

            var order = Order.Create(message.UserId, address, storeId);

            foreach (var cartItem in storeGroup)
            {
                var skuRef     = skuRefs[cartItem.SkuId];
                var productRef = productRefs[cartItem.ProductId];
                var unitPrice  = Money.FromVND(skuRef.Price);

                var snapshot = ProductSnapshot.Capture(
                    cartItem.ProductId,
                    cartItem.SkuId,
                    productRef.Name,
                    string.Empty,
                    unitPrice,
                    skuRef.ImageUrl ?? productRef.ThumbnailUrl ?? string.Empty,
                    new Dictionary<string, string>());

                order.AddItem(cartItem.ProductId, cartItem.SkuId, cartItem.Quantity, unitPrice, snapshot, isCOD: isCOD);

                allItemDtos.Add(new OrderItemDto
                {
                    ProductId   = cartItem.ProductId,
                    SkuId       = cartItem.SkuId,
                    StoreId     = storeId,
                    Quantity    = cartItem.Quantity,
                    Price       = skuRef.Price,
                    ProductName = productRef.Name,
                    SkuName     = string.Empty,
                    ImageUrl    = skuRef.ImageUrl ?? productRef.ThumbnailUrl ?? string.Empty
                });
            }

            order.SetShippingFee(Money.FromVND(pkgShipping), isShippingPaidBySeller: false);

            foreach (var coupon in coupons.Where(c =>
                c.OwnerType == CouponOwnerType.Store && c.StoreId == storeId))
            {
                order.ApplyDiscount(coupon);
            }

            orderRepository.Add(order);
            createdOrders.Add(order);
        }

        foreach (var platformCoupon in platformCoupons)
        {
            ApplyPlatformCouponProration(createdOrders, platformCoupon);
        }

        foreach (var order in createdOrders)
        {
            order.AddCheckout(domainPayment, order.TotalAmount);
        }

        await orderRepository.SaveChangesAsync(ct);

        var grandTotal = createdOrders.Sum(o => o.TotalAmount.Amount);

        logger.LogInformation("Created {OrderCount} orders for checkout {CorrelationId} (user {UserId})",
            createdOrders.Count, message.CorrelationId, message.UserId);

        await context.RespondAsync<OrderCreated>(new
        {
            message.CorrelationId,
            OrderIds      = createdOrders.Select(o => o.Id).ToList(),
            OrderStoreMap = createdOrders.ToDictionary(o => o.Id, o => o.StoreId),
            GrandTotal    = grandTotal,
            Items         = allItemDtos,
            CreatedAt     = createdOrders.First().CreatedAt
        });
    }

    private static void ApplyPlatformCouponProration(IReadOnlyList<Order> createdOrders, Coupon platformCoupon)
    {
        if (createdOrders.Count == 0)
            return;

        var weightedOrders = createdOrders
            .Where(o => o.SubTotal.Amount > 0)
            .Select(o => new
            {
                Order = o,
                Weight = o.SubTotal.Amount
            })
            .ToList();

        if (weightedOrders.Count == 0)
            return;

        var totalWeight = weightedOrders.Sum(x => x.Weight);
        var totalDiscountAmount = platformCoupon.CalculateDiscount(Money.FromVND(totalWeight)).Amount;

        if (totalDiscountAmount <= 0)
            return;

        var allocations = weightedOrders
            .Select(x => new
            {
                x.Order,
                BaseAmount = totalDiscountAmount * x.Weight / totalWeight,
                Remainder = totalDiscountAmount * x.Weight % totalWeight
            })
            .ToList();

        var allocatedAmount = allocations.Sum(x => x.BaseAmount);
        var remainingAmount = totalDiscountAmount - allocatedAmount;

        var amountByOrderId = allocations.ToDictionary(x => x.Order.Id, x => x.BaseAmount);

        if (remainingAmount > 0)
        {
            foreach (var allocation in allocations
                .OrderByDescending(x => x.Remainder)
                .Take((int)remainingAmount))
            {
                amountByOrderId[allocation.Order.Id] += 1;
            }
        }

        foreach (var weightedOrder in weightedOrders)
        {
            var allocated = amountByOrderId[weightedOrder.Order.Id];
            if (allocated <= 0)
                continue;

            weightedOrder.Order.ApplyProratedDiscount(platformCoupon, Money.FromVND(allocated));
        }
    }
}
