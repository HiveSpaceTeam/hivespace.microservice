using HiveSpace.OrderService.Domain.AggregateRoots;
using HiveSpace.OrderService.Domain.Entities;
using HiveSpace.OrderService.Domain.Enums;
using HiveSpace.OrderService.Domain.ValueObjects;

namespace HiveSpace.OrderService.Domain.Factories;

public static class OrderFactory
{
    public static Order CreateOrder(
        Guid customerId,
        double shippingFee,
        double discount,
        PaymentMethod paymentMethod,
        string shippingFullName,
        string shippingPhoneNumber,
        string shippingOtherDetails,
        string shippingStreet,
        string shippingWard,
        string shippingDistrict,
        string shippingProvince,
        string shippingCountry,
        IEnumerable<(int SkuId, string ProductName, string VariantName, string Thumbnail, int Quantity, double Amount, Currency Currency)> orderItems)
    {
        // Create shipping address
        var shippingAddress = new ShippingAddress(
            shippingFullName,
            shippingPhoneNumber,
            shippingOtherDetails,
            shippingStreet,
            shippingWard,
            shippingDistrict,
            shippingProvince,
            shippingCountry
        );

        // Create order items
        var items = orderItems.Select(item =>
            new OrderItem(
                item.SkuId,
                item.ProductName,
                item.VariantName,
                item.Thumbnail,
                item.Quantity,
                item.Amount,
                item.Currency
            )).ToList();

        // Create order
        return new Order(
            customerId,
            shippingFee,
            discount,
            DateTimeOffset.UtcNow,
            shippingAddress,
            paymentMethod,
            items
        );
    }

    public static OrderItem CreateOrderItem(
        int skuId,
        string productName,
        string variantName,
        string thumbnail,
        int quantity,
        double amount,
        Currency currency)
    {
        return new OrderItem(skuId, productName, variantName, thumbnail, quantity, amount, currency);
    }

    public static ShippingAddress CreateShippingAddress(
        string fullName,
        string phoneNumber,
        string otherDetails,
        string street,
        string ward,
        string district,
        string province,
        string country)
    {
        return new ShippingAddress(fullName, phoneNumber, otherDetails, street, ward, district, province, country);
    }
}