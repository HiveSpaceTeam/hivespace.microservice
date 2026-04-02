using HiveSpace.Application.Shared.Commands;
using HiveSpace.OrderService.Application.Coupons.Dtos;
using HiveSpace.OrderService.Domain.Enumerations;

namespace HiveSpace.OrderService.Application.Coupons.Commands.CreateCoupon;

public class CreateCouponCommand : ICommand<CouponDto>
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public DateTimeOffset StartDateTime { get; set; }
    public DateTimeOffset EndDateTime { get; set; }
    public DateTimeOffset? EarlySaveDateTime { get; set; }
    
    public DiscountType DiscountType { get; set; }
    public long? DiscountAmount { get; set; }
    public string DiscountCurrency { get; set; } = string.Empty;
    public decimal? DiscountPercentage { get; set; }
    public long? MaxDiscountAmount { get; set; }
    public long MinOrderAmount { get; set; }
    
    public CouponScope Scope { get; set; }
    
    public int MaxUsageCount { get; set; }
    public int CurrentUsageCount { get; set; }
    public int MaxUsagePerUser { get; set; }
    
    public bool IsHidden { get; set; }
    
    public List<long> ApplicableProductIds { get; set; } = new();
    public List<int> ApplicableCategoryIds { get; set; } = new();
}
