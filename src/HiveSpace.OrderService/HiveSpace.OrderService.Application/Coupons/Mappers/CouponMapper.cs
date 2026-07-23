using System.Linq;
using HiveSpace.Application.Shared.Dtos;
using HiveSpace.OrderService.Application.Coupons.Dtos;
using HiveSpace.OrderService.Domain.Aggregates.Coupons;
using HiveSpace.OrderService.Domain.Aggregates.Coupons.Specifications;
using HiveSpace.OrderService.Domain.Enumerations;
using HiveSpace.Domain.Shared.Enumerations;

namespace HiveSpace.OrderService.Application.Coupons.Mappers;

public static class CouponMapper
{
    public static CouponDto ToDto(this Coupon coupon)
    {
        return new CouponDto
        {
            Id = coupon.Id,
            Code = coupon.Code,
            Name = coupon.Name,
            StartDateTime = coupon.StartDateTime,
            EndDateTime = coupon.EndDateTime,
            EarlySaveDateTime = coupon.EarlySaveDateTime,
            DiscountType = coupon.DiscountType,
            DiscountAmount = coupon.DiscountAmount is null
                ? null
                : MoneyResponseDto.Valid(coupon.DiscountAmount.Amount, coupon.DiscountAmount.Currency.ToString()),
            CurrencyCode = coupon.CurrencyCode,
            DiscountPercentage = coupon.DiscountType == DiscountType.Percentage ? coupon.DiscountPercentage : null,
            MaxDiscountAmount = coupon.MaxDiscountAmount is null
                ? null
                : MoneyResponseDto.Valid(coupon.MaxDiscountAmount.Amount, coupon.MaxDiscountAmount.Currency.ToString()),
            MinOrderAmount = MoneyResponseDto.Valid(coupon.MinOrderAmount.Amount, coupon.MinOrderAmount.Currency.ToString()),
            Scope = coupon.Scope,
            MaxUsageCount = coupon.MaxUsageCount,
            CurrentUsageCount = coupon.CurrentUsageCount,
            MaxUsagePerUser = coupon.MaxUsagePerUser,
            IsHidden = coupon.IsHidden,
            OwnerType = coupon.OwnerType,
            CreatedBy = coupon.CreatedBy,
            IsActive = coupon.IsActive,
            CreatedAt = coupon.CreatedAt,
            UpdatedAt = coupon.UpdatedAt,
            ApplicableProductIds = coupon.ApplicableProductIds.ToList(),
            StoreId = coupon.StoreId,
            ApplicableCategoryIds = coupon.ApplicableCategoryIds.ToList(),
            Status = new CouponOngoingSpecification().IsSatisfiedBy(coupon) ? CouponStatus.Ongoing : 
                     new CouponUpcomingSpecification().IsSatisfiedBy(coupon) ? CouponStatus.Upcoming : 
                     CouponStatus.Expired
        };
    }

    public static CouponSummaryDto ToSummaryDto(this Coupon coupon)
    {
        return new CouponSummaryDto
        {
            Id = coupon.Id,
            Code = coupon.Code,
            Name = coupon.Name,
            StartDateTime = coupon.StartDateTime,
            EndDateTime = coupon.EndDateTime,
            DiscountType = coupon.DiscountType,
            DiscountAmount = coupon.DiscountAmount is null
                ? null
                : MoneyResponseDto.Valid(coupon.DiscountAmount.Amount, coupon.DiscountAmount.Currency.ToString()),
            CurrencyCode = coupon.CurrencyCode,
            DiscountPercentage = coupon.DiscountType == DiscountType.Percentage ? coupon.DiscountPercentage : null,
            MaxDiscountAmount = coupon.MaxDiscountAmount is null
                ? null
                : MoneyResponseDto.Valid(coupon.MaxDiscountAmount.Amount, coupon.MaxDiscountAmount.Currency.ToString()),
            MinOrderAmount = MoneyResponseDto.Valid(coupon.MinOrderAmount.Amount, coupon.MinOrderAmount.Currency.ToString()),
            MaxUsageCount = coupon.MaxUsageCount,
            CurrentUsageCount = coupon.CurrentUsageCount,
            IsHidden = coupon.IsHidden,
            IsActive = coupon.IsActive,
            ApplicableProductIds = coupon.ApplicableProductIds.ToList(),
            Status = new CouponOngoingSpecification().IsSatisfiedBy(coupon) ? CouponStatus.Ongoing : 
                     new CouponUpcomingSpecification().IsSatisfiedBy(coupon) ? CouponStatus.Upcoming : 
                     CouponStatus.Expired
        };
    }
}
