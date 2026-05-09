namespace HiveSpace.OrderService.Application.Orders.Dtos;

public record OrderDetailDto
{
    public Guid    Id            { get; init; }
    public string  OrderCode     { get; init; } = null!;
    public Guid    UserId        { get; init; }
    public Guid    StoreId       { get; init; }
    public string  Status        { get; init; } = null!;
    public long    SubTotal      { get; init; }
    public long    TotalDiscount { get; init; }
    public long    ShippingFee   { get; init; }
    public long    TotalAmount   { get; init; }
    public string  Currency      { get; init; } = null!;
    public string? PaymentMethod { get; init; }
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
