using System;
using System.Collections.Generic;
using HiveSpace.Application.Shared.Dtos;
using HiveSpace.OrderService.Domain.Enumerations;

namespace HiveSpace.OrderService.Application.Coupons.Dtos;

/// <summary>
/// Lean DTO for the coupon list view.
/// Contains only fields consumed by the CouponList page.
/// For full coupon details, use <see cref="CouponDto"/>.
/// </summary>
public record CouponSummaryDto
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

    public int MaxUsageCount { get; init; }
    public int CurrentUsageCount { get; init; }

    public bool IsHidden { get; init; }
    public bool IsActive { get; init; }

    public IReadOnlyCollection<long> ApplicableProductIds { get; init; } = [];

    public CouponStatus Status { get; init; }
}
