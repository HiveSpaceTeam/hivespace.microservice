namespace HiveSpace.OrderService.Application.Orders.Dtos;

public record OrderDetailDto
{
    public Guid    Id            { get; init; }
    public string  ShortId       { get; init; } = null!;
    public Guid    UserId        { get; init; }
    public Guid    StoreId       { get; init; }
    public string  Status        { get; init; } = null!;
    public long    SubTotal      { get; init; }
    public long    ShippingFee   { get; init; }
    public long    TotalAmount   { get; init; }
    public string  Currency      { get; init; } = null!;
    public string? PaymentMethod { get; init; }
    public string  RecipientName  { get; init; } = null!;
    public string  Phone          { get; init; } = null!;
    public string  StreetAddress  { get; init; } = null!;
    public string  Commune        { get; init; } = null!;
    public string  Province       { get; init; } = null!;
    public string  Country        { get; init; } = null!;
    public string? Notes          { get; init; }

    public DateTimeOffset  CreatedAt   { get; init; }
    public DateTimeOffset? PaidAt      { get; init; }
    public DateTimeOffset? ConfirmedAt { get; init; }

    public List<OrderItemSummaryDto> Items { get; init; } = new();
}
