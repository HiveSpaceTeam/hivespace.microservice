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
using SharedPaymentMethod = HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.PaymentMethod;

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

        var domainPaymentMethod = message.PaymentMethod == SharedPaymentMethod.COD
            ? DomainPaymentMethod.COD
            : DomainPaymentMethod.COD;

        var itemsByStore = message.Items.GroupBy(i => i.StoreId);
        foreach (var storeGroup in itemsByStore)
        {
            var package = OrderPackage.Create(storeGroup.Key, message.UserId);
            foreach (var item in storeGroup)
            {
                var productGuid = ToGuid(item.ProductId);
                var skuGuid     = ToGuid(item.SkuId);
                var unitPrice       = Money.FromVND((long)item.Price);
                var snapshotPrice   = Money.FromVND((long)item.Price);

                var snapshot = ProductSnapshot.Capture(
                    productGuid,
                    skuGuid,
                    item.ProductName,
                    item.SkuName,
                    snapshotPrice,
                    item.ImageUrl,
                    new Dictionary<string, string>());

                package.AddItem(productGuid, skuGuid, item.Quantity, unitPrice, snapshot, isCOD: true);
            }
            package.SetShippingFee(Money.FromVND(0), isShippingPaidBySeller: false);
            package.AddCheckout(domainPaymentMethod, package.GetCODAmount());
            order.AddPackage(package);
        }

        orderRepository.Add(order);
        await orderRepository.SaveChangesAsync(ct);

        logger.LogInformation("Order {OrderId} created for user {UserId} with {PackageCount} packages",
            order.Id, message.UserId, order.Packages.Count);

        await context.Publish<OrderCreated>(new
        {
            message.CorrelationId,
            OrderId    = order.Id,
            PackageIds = order.Packages.Select(p => p.Id).ToList(),
            CreatedAt  = order.CreatedAt
        });
    }

    private static Guid ToGuid(long value)
    {
        var bytes = new byte[16];
        BitConverter.GetBytes(value).CopyTo(bytes, 0);
        return new Guid(bytes);
    }
}
