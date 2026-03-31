using HiveSpace.Domain.Shared.ValueObjects;
using HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.Commands;
using HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.Dtos;
using HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.Events;
using HiveSpace.OrderService.Domain.Aggregates.Orders;
using HiveSpace.OrderService.Domain.Repositories;
using HiveSpace.OrderService.Domain.ValueObjects;
using MassTransit;
using Microsoft.Extensions.Logging;
using DomainPaymentMethod = HiveSpace.OrderService.Domain.Enumerations.PaymentMethod;
using static HiveSpace.OrderService.Application.Cart.CheckoutCalculator;

namespace HiveSpace.OrderService.Api.Consumers.Saga.CheckoutSaga;

public class CreateOrderConsumer(
    IOrderRepository orderRepository,
    ICartRepository cartRepository,
    IProductRefRepository productRefRepository,
    ISkuRefRepository skuRefRepository,
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

        var itemsByStore     = selectedItems.GroupBy(i => productRefs[i.ProductId].StoreId).ToList();
        var shippingPerStore = DistributeShippingFee(
            CalculateShippingFee(selectedItems.Sum(i => i.Quantity)), itemsByStore.Count);

        var createdOrders = new List<Order>();
        var allItemDtos   = new List<OrderItemDto>();

        for (int i = 0; i < itemsByStore.Count; i++)
        {
            var storeGroup  = itemsByStore[i];
            var storeId     = storeGroup.Key;
            var pkgShipping = shippingPerStore[i];

            var order = Order.Create(storeId, message.UserId, address);

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

                order.AddItem(cartItem.ProductId, cartItem.SkuId, cartItem.Quantity, unitPrice, snapshot, isCOD: true);

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
            order.AddCheckout(DomainPaymentMethod.COD, order.GetCODAmount());

            orderRepository.Add(order);
            createdOrders.Add(order);
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
}
