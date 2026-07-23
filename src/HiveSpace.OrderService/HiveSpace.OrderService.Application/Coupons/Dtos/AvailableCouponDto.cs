using HiveSpace.Application.Shared.Dtos;
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
    public MoneyResponseDto? DiscountAmount { get; init; }
    public string CurrencyCode { get; init; } = string.Empty;
    public decimal? DiscountPercentage { get; init; }
    public MoneyResponseDto? MaxDiscountAmount { get; init; }
    public MoneyResponseDto MinOrderAmount { get; init; } = null!;
    public CouponScope Scope { get; init; }
    public bool IsApplicable { get; init; }
}
