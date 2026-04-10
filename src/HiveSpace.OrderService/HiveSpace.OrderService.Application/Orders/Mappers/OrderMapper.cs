using HiveSpace.OrderService.Application.Orders.Dtos;
using HiveSpace.OrderService.Domain.Aggregates.Orders;

namespace HiveSpace.OrderService.Application.Orders.Mappers;

public static class OrderMapper
{
    public static OrderDetailDto ToDetailDto(this Order order) => new()
    {
        Id            = order.Id,
        ShortId       = order.ShortId,
        UserId        = order.UserId,
        StoreId       = order.StoreId,
        Status        = order.Status.Name,
        SubTotal      = order.SubTotal.Amount,
        TotalAmount   = order.TotalAmount.Amount,
        Currency      = order.TotalAmount.Currency.ToString(),
        RecipientName = order.DeliveryAddress.RecipientName,
        Phone         = order.DeliveryAddress.Phone.Value,
        StreetAddress = order.DeliveryAddress.StreetAddress,
        Commune       = order.DeliveryAddress.Commune,
        Province      = order.DeliveryAddress.Province,
        Country       = order.DeliveryAddress.Country,
        Notes         = order.DeliveryAddress.Notes,
        CreatedAt     = order.CreatedAt,
        PaidAt        = order.PaidAt,
        ConfirmedAt   = order.ConfirmedAt,
        Items         = order.Items.Select(i => i.ToSummaryDto(order.TotalAmount.Currency.ToString())).ToList()
    };

    public static OrderItemSummaryDto ToSummaryDto(this OrderItem item, string currency) => new()
    {
        Id          = item.Id,
        ProductId   = item.ProductId,
        SkuId       = item.SkuId,
        ProductName = item.ProductSnapshot.ProductName,
        ImageUrl    = item.ProductSnapshot.ImageUrl,
        Quantity    = item.Quantity,
        UnitPrice   = item.UnitPrice.Amount,
        LineTotal   = item.LineTotal.Amount,
        Currency    = currency,
        IsCOD       = item.IsCOD
    };

    public static CustomerOrderSummaryDto ToCustomerSummaryDto(this Order order)
    {
        var currency = order.TotalAmount.Currency.ToString();
        return new CustomerOrderSummaryDto
        {
            Id          = order.Id,
            ShortId     = order.ShortId,
            Status      = order.Status.Name,
            TotalAmount = order.TotalAmount.Amount,
            Currency    = currency,
            CreatedAt   = order.CreatedAt,
            ItemCount   = order.Items.Count,
            Items       = order.Items.Select(i => i.ToCustomerItemDto(currency)).ToList()
        };
    }

    public static CustomerOrderItemDto ToCustomerItemDto(this OrderItem item, string currency) => new()
    {
        Id            = item.Id,
        ProductName   = item.ProductSnapshot.ProductName,
        ProductImage  = item.ProductSnapshot.ImageUrl,
        Variation     = item.ProductSnapshot.SkuName,
        Quantity      = item.Quantity,
        OriginalPrice = item.ProductSnapshot.Price.Amount,
        UnitPrice     = item.UnitPrice.Amount,
        LineTotal     = item.LineTotal.Amount,
        Currency      = currency
    };

    public static SellerOrderSummaryDto ToSellerSummaryDto(this Order order) => new()
    {
        Id            = order.Id,
        OrderCode     = order.ShortId,
        BuyerName     = order.DeliveryAddress.RecipientName,
        Status        = order.Status.Name,
        PaymentMethod = order.Checkouts.FirstOrDefault()?.PaymentMethod.Name,
        TotalAmount   = order.TotalAmount.Amount,
        ActionDateTime = order.UpdatedAt ?? order.CreatedAt,
        CreatedAt     = order.CreatedAt,
        Items         = order.Items.Select(i => i.ToSellerItemDto()).ToList()
    };

    public static SellerOrderItemDto ToSellerItemDto(this OrderItem item) => new()
    {
        Id           = item.Id,
        ProductName  = item.ProductSnapshot.ProductName,
        ProductImageUrl = item.ProductSnapshot.ImageUrl,
        Variation    = item.ProductSnapshot.SkuName,
        Quantity     = item.Quantity,
        Tag          = item.IsCOD ? "cod" : null
    };
}
