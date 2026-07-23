using HiveSpace.Application.Shared.Dtos;
using HiveSpace.OrderService.Application.Orders.Dtos;
using HiveSpace.OrderService.Domain.Aggregates.Orders;

namespace HiveSpace.OrderService.Application.Orders.Mappers;

public static class OrderMapper
{
    public static OrderDetailDto ToDetailDto(this Order order) => new()
    {
        Id            = order.Id,
        OrderCode     = order.OrderCode,
        UserId        = order.UserId,
        StoreId       = order.StoreId,
        Status        = order.Status.Name,
        SubTotal      = MoneyResponseDto.Valid(order.SubTotal.Amount, order.SubTotal.Currency.ToString()),
        TotalDiscount = MoneyResponseDto.Valid(order.TotalDiscount.Amount, order.TotalDiscount.Currency.ToString()),
        ShippingFee   = MoneyResponseDto.Valid(order.ShippingFee.Amount, order.ShippingFee.Currency.ToString()),
        TotalAmount   = MoneyResponseDto.Valid(order.TotalAmount.Amount, order.TotalAmount.Currency.ToString()),
        PaymentMethod = order.PaymentMethodCode ?? order.Checkouts.FirstOrDefault()?.PaymentMethod.Name,
        PaymentId = order.PaymentId,
        PaymentReferenceNo = order.PaymentReferenceNo,
        PaymentMethodCode = order.PaymentMethodCode,
        PaymentStatus = order.Status.Name,
        PaymentAttemptId = order.PaymentAttemptId,
        PaymentAttemptNo = order.PaymentAttemptNo,
        IsShippingPaidBySeller = order.IsShippingPaidBySeller,
        ShippingId    = order.ShippingId,
        RejectionReason = order.RejectionReason,
        RecipientName = order.DeliveryAddress.RecipientName,
        Phone         = order.DeliveryAddress.Phone.Value,
        StreetAddress = order.DeliveryAddress.StreetAddress,
        Commune       = order.DeliveryAddress.Commune,
        Province      = order.DeliveryAddress.Province,
        Country       = order.DeliveryAddress.Country,
        Notes         = order.DeliveryAddress.Notes,
        CreatedAt     = order.CreatedAt,
        UpdatedAt     = order.UpdatedAt,
        PaidAt        = order.PaidAt,
        ConfirmedAt   = order.ConfirmedAt,
        RejectedAt    = order.RejectedAt,
        ExpiredAt     = order.ExpiredAt,
        Items         = order.Items.Select(i => i.ToSummaryDto()).ToList(),
        Checkouts     = order.Checkouts.Select(c => c.ToDto()).ToList(),
        Discounts     = order.Discounts.Select(d => d.ToDto()).ToList(),
        Trackings     = order.Trackings.OrderBy(t => t.CreatedAt).Select(t => t.ToDto()).ToList()
    };

    public static OrderItemSummaryDto ToSummaryDto(this OrderItem item) => new()
    {
        Id                = item.Id,
        ProductId         = item.ProductId,
        SkuId             = item.SkuId,
        ProductName       = item.ProductSnapshot.ProductName,
        SkuName           = item.ProductSnapshot.SkuName,
        ImageUrl          = item.ProductSnapshot.ImageUrl,
        Quantity          = item.Quantity,
        UnitPrice         = MoneyResponseDto.Valid(item.UnitPrice.Amount, item.UnitPrice.Currency.ToString()),
        LineTotal         = MoneyResponseDto.Valid(item.LineTotal.Amount, item.LineTotal.Currency.ToString()),
        IsCOD             = item.IsCOD,
        SnapshotPrice     = MoneyResponseDto.Valid(item.ProductSnapshot.Price.Amount, item.ProductSnapshot.Currency.ToString()),
        SnapshotCapturedAt = item.ProductSnapshot.CapturedAt,
        Attributes        = item.ProductSnapshot.Attributes.ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
    };

    public static OrderCheckoutDto ToDto(this Checkout checkout) => new()
    {
        PaymentMethod = checkout.PaymentMethod.Name,
        Amount = MoneyResponseDto.Valid(checkout.Amount.Amount, checkout.Amount.Currency.ToString()),
        CreatedAt = checkout.CreatedAt
    };

    public static OrderDiscountDto ToDto(this Discount discount) => new()
    {
        CouponId = discount.CouponId,
        CouponCode = discount.CouponCode,
        CouponOwnerType = discount.CouponOwnerType.ToString(),
        Scope = discount.Scope.ToString(),
        Amount = MoneyResponseDto.Valid(discount.DiscountAmount.Amount, discount.DiscountAmount.Currency.ToString()),
        AppliedAt = discount.AppliedAt
    };

    public static OrderTrackingDto ToDto(this OrderTracking tracking) => new()
    {
        Id = tracking.Id,
        Type = tracking.Type,
        ExecutorType = tracking.ExecutorType.ToString(),
        ExecutorId = tracking.ExecutorId,
        Message = tracking.Message,
        CreatedAt = tracking.CreatedAt
    };

    public static BuyerOrderSummaryDto ToBuyerSummaryDto(this Order order)
    {
        return new BuyerOrderSummaryDto
        {
            Id          = order.Id,
            OrderCode   = order.OrderCode,
            Status      = order.Status.Name,
            PaymentId = order.PaymentId,
            PaymentReferenceNo = order.PaymentReferenceNo,
            PaymentMethodCode = order.PaymentMethodCode,
            PaymentAttemptNo = order.PaymentAttemptNo,
            TotalAmount = MoneyResponseDto.Valid(order.TotalAmount.Amount, order.TotalAmount.Currency.ToString()),
            CreatedAt   = order.CreatedAt,
            ItemCount   = order.Items.Count,
            Items       = order.Items.Select(i => i.ToBuyerItemDto()).ToList()
        };
    }

    public static BuyerOrderItemDto ToBuyerItemDto(this OrderItem item) => new()
    {
        Id            = item.Id,
        ProductName   = item.ProductSnapshot.ProductName,
        ProductImage  = item.ProductSnapshot.ImageUrl,
        Variation     = item.ProductSnapshot.SkuName,
        Quantity      = item.Quantity,
        OriginalPrice = MoneyResponseDto.Valid(item.ProductSnapshot.Price.Amount, item.ProductSnapshot.Currency.ToString()),
        UnitPrice     = MoneyResponseDto.Valid(item.UnitPrice.Amount, item.UnitPrice.Currency.ToString()),
        LineTotal     = MoneyResponseDto.Valid(item.LineTotal.Amount, item.LineTotal.Currency.ToString())
    };

    public static SellerOrderSummaryDto ToSellerSummaryDto(this Order order) => new()
    {
        Id            = order.Id,
        OrderCode     = order.OrderCode,
        BuyerName     = order.DeliveryAddress.RecipientName,
        Status        = order.Status.Name,
        PaymentMethod = order.PaymentMethodCode ?? order.Checkouts.FirstOrDefault()?.PaymentMethod.Name,
        PaymentId = order.PaymentId,
        PaymentReferenceNo = order.PaymentReferenceNo,
        PaymentMethodCode = order.PaymentMethodCode,
        PaymentAttemptNo = order.PaymentAttemptNo,
        TotalAmount   = MoneyResponseDto.Valid(order.TotalAmount.Amount, order.TotalAmount.Currency.ToString()),
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
