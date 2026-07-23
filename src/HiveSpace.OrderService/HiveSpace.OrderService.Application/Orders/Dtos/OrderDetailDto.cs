using HiveSpace.Application.Shared.Dtos;

namespace HiveSpace.OrderService.Application.Orders.Dtos;

public record OrderDetailDto
{
    public Guid    Id            { get; init; }
    public string  OrderCode     { get; init; } = null!;
    public Guid    UserId        { get; init; }
    public Guid    StoreId       { get; init; }
    public string  Status        { get; init; } = null!;
    public MoneyResponseDto SubTotal      { get; init; } = null!;
    public MoneyResponseDto TotalDiscount { get; init; } = null!;
    public MoneyResponseDto ShippingFee   { get; init; } = null!;
    public MoneyResponseDto TotalAmount   { get; init; } = null!;
    public string? PaymentMethod { get; init; }
    public Guid?   PaymentId { get; init; }
    public string? PaymentReferenceNo { get; init; }
    public string? PaymentMethodCode { get; init; }
    public string? PaymentStatus { get; init; }
    public Guid?   PaymentAttemptId { get; init; }
    public int?    PaymentAttemptNo { get; init; }
    public bool    IsShippingPaidBySeller { get; init; }
    public Guid?   ShippingId { get; init; }
    public string? RejectionReason { get; init; }
    public string  RecipientName  { get; init; } = null!;
    public string  Phone          { get; init; } = null!;
    public string  StreetAddress  { get; init; } = null!;
    public string  Commune        { get; init; } = null!;
    public string  Province       { get; init; } = null!;
    public string  Country        { get; init; } = null!;
    public string? Notes          { get; init; }

    public DateTimeOffset  CreatedAt   { get; init; }
    public DateTimeOffset? UpdatedAt   { get; init; }
    public DateTimeOffset? PaidAt      { get; init; }
    public DateTimeOffset? ConfirmedAt { get; init; }
    public DateTimeOffset? RejectedAt  { get; init; }
    public DateTimeOffset? ExpiredAt   { get; init; }

    public List<OrderItemSummaryDto> Items { get; init; } = new();
    public List<OrderCheckoutDto> Checkouts { get; init; } = new();
    public List<OrderDiscountDto> Discounts { get; init; } = new();
    public List<OrderTrackingDto> Trackings { get; init; } = new();
}
