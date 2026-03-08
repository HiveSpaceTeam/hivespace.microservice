using System;
using System.Collections.Generic;
using HiveSpace.OrderService.Domain.Enumerations;

namespace HiveSpace.OrderService.Application.Coupons.Dtos;

public record CouponDto
{
    public Guid Id { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public DateTimeOffset StartDateTime { get; init; }
    public DateTimeOffset EndDateTime { get; init; }
    public DateTimeOffset? EarlySaveDateTime { get; init; }
    
    public DiscountType DiscountType { get; init; }
    public long? DiscountAmount { get; init; }
    public string DiscountCurrency { get; init; } = string.Empty;
    public decimal? DiscountPercentage { get; init; }
    public long? MaxDiscountAmount { get; init; }
    public long MinOrderAmount { get; init; }
    
    public CouponScope Scope { get; init; }
    public int MaxUsageCount { get; init; }
    public int CurrentUsageCount { get; init; }
    public int MaxUsagePerUser { get; init; }
    
    public bool IsHidden { get; init; }
    public CouponOwnerType OwnerType { get; init; }
    
    public string CreatedBy { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? UpdatedAt { get; init; }
    
    public IReadOnlyCollection<long> ApplicableProductIds { get; init; } = [];
    public Guid? StoreId { get; init; }
    public IReadOnlyCollection<int> ApplicableCategoryIds { get; init; } = [];

    public CouponStatus Status { get; init; }
}
