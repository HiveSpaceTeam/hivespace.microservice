using HiveSpace.OrderService.Domain.Enumerations;

namespace HiveSpace.OrderService.Application.Coupons.Dtos;

public record AvailableCouponDto
{
    public Guid Id { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public DateTimeOffset StartDateTime { get; init; }
    public DateTimeOffset EndDateTime { get; init; }
    public DiscountType DiscountType { get; init; }
    public long? DiscountAmount { get; init; }
    public string DiscountCurrency { get; init; } = string.Empty;
    public decimal? DiscountPercentage { get; init; }
    public long? MaxDiscountAmount { get; init; }
    public long MinOrderAmount { get; init; }
    public CouponScope Scope { get; init; }
    public bool IsApplicable { get; init; }
}
