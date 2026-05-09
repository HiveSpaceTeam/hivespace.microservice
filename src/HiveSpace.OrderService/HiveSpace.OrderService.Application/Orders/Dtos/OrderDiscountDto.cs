namespace HiveSpace.OrderService.Application.Orders.Dtos;

public record OrderDiscountDto
{
    public Guid CouponId { get; init; }
    public string CouponCode { get; init; } = null!;
    public string CouponOwnerType { get; init; } = null!;
    public string Scope { get; init; } = null!;
    public long Amount { get; init; }
    public string Currency { get; init; } = null!;
    public DateTimeOffset AppliedAt { get; init; }
}
