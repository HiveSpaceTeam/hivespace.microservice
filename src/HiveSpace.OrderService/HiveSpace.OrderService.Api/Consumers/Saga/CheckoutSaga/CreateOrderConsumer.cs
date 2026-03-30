using HiveSpace.Domain.Shared.ValueObjects;
using HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.Commands;
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
    ILogger<CreateOrderConsumer> logger) : IConsumer<CreateOrder>
{
    public async Task Consume(ConsumeContext<CreateOrder> context)
    {
        var message = context.Message;
        var ct = context.CancellationToken;

        var phone   = new PhoneNumber(message.DeliveryAddress.Phone);
        var address = new DeliveryAddress(
            message.DeliveryAddress.RecipientName,
            phone,
            message.DeliveryAddress.StreetAddress,
            message.DeliveryAddress.Commune,
            message.DeliveryAddress.Province,
            message.DeliveryAddress.Country,
            message.DeliveryAddress.Notes ?? string.Empty);

        var order = Order.Create(message.UserId, address, id: message.CorrelationId);

        var itemsByStore       = message.Items.GroupBy(i => i.StoreId).ToList();
        var shippingPerStore   = DistributeShippingFee(message.ShippingFee, itemsByStore.Count);

        for (int i = 0; i < itemsByStore.Count; i++)
        {
            var storeGroup  = itemsByStore[i];
            var pkgShipping = shippingPerStore[i];
            var package     = OrderPackage.Create(storeGroup.Key, message.UserId);

            foreach (var item in storeGroup)
            {
                var unitPrice     = Money.FromVND(item.Price);
                var snapshotPrice = Money.FromVND(item.Price);

                var snapshot = ProductSnapshot.Capture(
                    item.ProductId,
                    item.SkuId,
                    item.ProductName,
                    item.SkuName,
                    snapshotPrice,
                    item.ImageUrl,
                    new Dictionary<string, string>());

                package.AddItem(item.ProductId, item.SkuId, item.Quantity, unitPrice, snapshot, isCOD: true);
            }

            package.SetShippingFee(Money.FromVND(pkgShipping), isShippingPaidBySeller: false);
            package.AddCheckout(DomainPaymentMethod.COD, package.GetCODAmount());
            order.AddPackage(package);
        }

        orderRepository.Add(order);
        await orderRepository.SaveChangesAsync(ct);

        logger.LogInformation("Order {OrderId} created for user {UserId} with {PackageCount} packages",
            order.Id, message.UserId, order.Packages.Count);

        await context.RespondAsync<OrderCreated>(new
        {
            message.CorrelationId,
            OrderId    = order.Id,
            PackageIds = order.Packages.Select(p => p.Id).ToList(),
            CreatedAt  = order.CreatedAt
        });
    }

}
