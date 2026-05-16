using HiveSpace.Domain.Shared.Errors;
using HiveSpace.OrderService.Application.Cart.Dtos;
using HiveSpace.OrderService.Domain.Aggregates.Coupons;
using HiveSpace.OrderService.Domain.Enumerations;
using HiveSpace.OrderService.Domain.Exceptions;
using HiveSpace.OrderService.Domain.Repositories;
using HiveSpace.Core.Exceptions.Models;
using static HiveSpace.OrderService.Application.Cart.CheckoutCalculator;
using CartAggregate = HiveSpace.OrderService.Domain.Aggregates.Carts.Cart;

namespace HiveSpace.OrderService.Application.Cart;

public static class PersistedCartCouponState
{
    public static async Task RemoveInvalidStoreCouponsAsync(
        CartAggregate cart,
        IReadOnlyCollection<SelectedCartStoreSnapshot> snapshots,
        ICouponRepository couponRepository,
        Guid userId,
        CancellationToken cancellationToken)
    {
        var storeSelections = cart.AppliedStoreCoupons
            .GroupBy(x => x.StoreId)
            .Select(x => x.Last())
            .ToList();

        if (storeSelections.Count == 0)
            return;

        var couponCodes = storeSelections
            .Select(x => x.CouponCode)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var coupons = await couponRepository.GetByCodesAsync(couponCodes, cancellationToken);
        var couponsByCode = coupons.ToDictionary(x => x.Code, StringComparer.OrdinalIgnoreCase);

        foreach (var selection in storeSelections)
        {
            var snapshot = snapshots.FirstOrDefault(x => x.StoreId == selection.StoreId);
            if (!TryValidateStoreCoupon(selection, snapshot, couponsByCode, userId, out _, out _))
                cart.RemoveStoreCoupon(selection.StoreId);
        }
    }

    public static async Task<CartCouponValidationResult> ValidateAsync(
        CartAggregate cart,
        IReadOnlyCollection<SelectedCartStoreSnapshot> snapshots,
        ICouponRepository couponRepository,
        Guid userId,
        CancellationToken cancellationToken,
        bool removeInvalidSelections)
    {
        var platformCodes = cart.AppliedPlatformCoupons
            .Select(x => x.CouponCode)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        var storeSelections = cart.AppliedStoreCoupons
            .GroupBy(x => x.StoreId)
            .Select(x => x.Last())
            .ToList();

        var allCodes = platformCodes
            .Concat(storeSelections.Select(x => x.CouponCode))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var coupons = allCodes.Count > 0
            ? await couponRepository.GetByCodesAsync(allCodes, cancellationToken)
            : [];
        var couponsByCode = coupons.ToDictionary(x => x.Code, StringComparer.OrdinalIgnoreCase);
        var invalidatedCoupons = new List<InvalidAppliedCouponDto>();
        var appliedPlatformCoupons = new List<AppliedPlatformCouponDto>();
        var appliedStoreCoupons = new Dictionary<Guid, AppliedStoreCouponDto>();
        var grandSubtotal = snapshots.Sum(x => x.Subtotal);

        foreach (var couponCode in platformCodes)
        {
            if (!TryValidatePlatformCoupon(couponCode, couponsByCode, userId, grandSubtotal, out var applied, out var invalid))
            {
                if (invalid is not null)
                    invalidatedCoupons.Add(invalid);

                if (removeInvalidSelections)
                    cart.RemovePlatformCoupon(couponCode);

                continue;
            }

            appliedPlatformCoupons.Add(applied!);
        }

        foreach (var selection in storeSelections)
        {
            var snapshot = snapshots.FirstOrDefault(x => x.StoreId == selection.StoreId);
            if (!TryValidateStoreCoupon(selection, snapshot, couponsByCode, userId, out var applied, out var invalid))
            {
                if (invalid is not null)
                    invalidatedCoupons.Add(invalid);

                if (removeInvalidSelections)
                    cart.RemoveStoreCoupon(selection.StoreId);

                continue;
            }

            appliedStoreCoupons[selection.StoreId] = applied!;
        }

        return new CartCouponValidationResult(
            appliedPlatformCoupons,
            appliedStoreCoupons,
            invalidatedCoupons,
            couponsByCode);
    }

    public static Exception BuildCheckoutCouponException(IReadOnlyCollection<InvalidAppliedCouponDto> invalidCoupons)
    {
        var errors = invalidCoupons
            .Select(x => new Error(OrderDomainErrorCode.CheckoutValidationFailed, x.CouponCode))
            .ToList();

        return new HiveSpace.Core.Exceptions.BadRequestException(errors);
    }

    private static bool TryValidatePlatformCoupon(
        string couponCode,
        IReadOnlyDictionary<string, Coupon> couponsByCode,
        Guid userId,
        long grandSubtotal,
        out AppliedPlatformCouponDto? applied,
        out InvalidAppliedCouponDto? invalid)
    {
        applied = null;
        invalid = null;

        if (!couponsByCode.TryGetValue(couponCode, out var coupon))
        {
            invalid = BuildInvalidCoupon(couponCode, CouponOwnerType.Platform, null, OrderDomainErrorCode.CouponNotFound);
            return false;
        }

        if (coupon.OwnerType != CouponOwnerType.Platform)
        {
            invalid = BuildInvalidCoupon(couponCode, CouponOwnerType.Platform, null, OrderDomainErrorCode.CouponStoreNotApplicable);
            return false;
        }

        var validation = coupon.Validate(userId, HiveSpace.Domain.Shared.ValueObjects.Money.FromVND(grandSubtotal));
        if (!validation.IsValid)
        {
            invalid = BuildInvalidCoupon(couponCode, CouponOwnerType.Platform, null, validation.Errors.First().ErrorCode);
            return false;
        }

        applied = new AppliedPlatformCouponDto(coupon.Code);
        return true;
    }

    private static bool TryValidateStoreCoupon(
        HiveSpace.OrderService.Domain.Aggregates.Carts.CartAppliedStoreCoupon selection,
        SelectedCartStoreSnapshot? snapshot,
        IReadOnlyDictionary<string, Coupon> couponsByCode,
        Guid userId,
        out AppliedStoreCouponDto? applied,
        out InvalidAppliedCouponDto? invalid)
    {
        applied = null;
        invalid = null;

        if (snapshot is null)
        {
            invalid = BuildInvalidCoupon(
                selection.CouponCode,
                CouponOwnerType.Store,
                selection.StoreId,
                OrderDomainErrorCode.CouponStoreNotApplicable);
            return false;
        }

        if (!couponsByCode.TryGetValue(selection.CouponCode, out var coupon))
        {
            invalid = BuildInvalidCoupon(selection.CouponCode, CouponOwnerType.Store, selection.StoreId, OrderDomainErrorCode.CouponNotFound);
            return false;
        }

        if (coupon.OwnerType != CouponOwnerType.Store || coupon.StoreId != selection.StoreId)
        {
            invalid = BuildInvalidCoupon(selection.CouponCode, CouponOwnerType.Store, selection.StoreId, OrderDomainErrorCode.CouponStoreNotApplicable);
            return false;
        }

        var evaluation = SelectedCartCouponEvaluator.EvaluateCoupon(coupon, userId, snapshot);
        if (!evaluation.IsApplicable)
        {
            var errorCode = evaluation.Errors.FirstOrDefault() ?? OrderDomainErrorCode.CouponInvalid;
            invalid = BuildInvalidCoupon(selection.CouponCode, CouponOwnerType.Store, selection.StoreId, errorCode);
            return false;
        }

        applied = new AppliedStoreCouponDto(selection.StoreId, coupon.Code);
        return true;
    }

    public static InvalidAppliedCouponDto BuildInvalidCoupon(
        string couponCode,
        CouponOwnerType ownerType,
        Guid? storeId,
        DomainErrorCode errorCode)
        => new(couponCode, ownerType, storeId, errorCode.Code, errorCode.Name);
}

public record CartCouponValidationResult(
    List<AppliedPlatformCouponDto> AppliedPlatformCoupons,
    Dictionary<Guid, AppliedStoreCouponDto> AppliedStoreCoupons,
    List<InvalidAppliedCouponDto> InvalidatedCoupons,
    IReadOnlyDictionary<string, Coupon> CouponsByCode
);
