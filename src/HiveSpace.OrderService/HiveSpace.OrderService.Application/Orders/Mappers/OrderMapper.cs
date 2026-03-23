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
        Status        = order.Status.Name,
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
        Packages      = order.Packages.Select(p => p.ToDetailDto()).ToList()
    };

    public static PackageDetailDto ToDetailDto(this OrderPackage package) => new()
    {
        Id          = package.Id,
        StoreId     = package.StoreId,
        Status      = package.Status.Name,
        SubTotal    = package.SubTotal.Amount,
        TotalAmount = package.TotalAmount.Amount,
        Currency    = package.TotalAmount.Currency.ToString(),
        ItemCount   = package.Items.Count,
        CreatedAt   = package.CreatedAt
    };

    public static OrderSummaryDto ToSummaryDto(this Order order) => new()
    {
        Id           = order.Id,
        ShortId      = order.ShortId,
        Status       = order.Status.Name,
        TotalAmount  = order.TotalAmount.Amount,
        Currency     = order.TotalAmount.Currency.ToString(),
        CreatedAt    = order.CreatedAt,
        PackageCount = order.Packages.Count
    };
}
