using HiveSpace.Application.Shared.Commands;
using HiveSpace.OrderService.Application.Coupons.Dtos;

namespace HiveSpace.OrderService.Application.Coupons.Commands.UpdateCoupon;

public record UpdateCouponCommand : ICommand<CouponDto>
{
    public Guid Id { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public DateTimeOffset StartDateTime { get; init; }
    public DateTimeOffset EndDateTime { get; init; }
    public DateTimeOffset? EarlySaveDateTime { get; init; }
    
    public string DiscountCurrency { get; init; } = string.Empty;
    public long? DiscountAmount { get; init; }
    public decimal? DiscountPercentage { get; init; }
    public long? MaxDiscountAmount { get; init; }
    public long MinOrderAmount { get; init; }
    
    public int MaxUsageCount { get; init; }
    
    public IReadOnlyCollection<long> ApplicableProductIds { get; init; } = [];
}
