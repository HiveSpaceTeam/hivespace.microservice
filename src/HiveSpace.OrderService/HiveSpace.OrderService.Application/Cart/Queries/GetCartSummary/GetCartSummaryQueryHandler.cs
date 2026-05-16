using HiveSpace.Application.Shared.Handlers;
using HiveSpace.Core.Contexts;
using HiveSpace.OrderService.Application.Cart.Dtos;
using HiveSpace.OrderService.Domain.Aggregates.Carts;
using HiveSpace.OrderService.Domain.Aggregates.Coupons;
using HiveSpace.OrderService.Domain.Enumerations;
using HiveSpace.OrderService.Domain.Repositories;
using static HiveSpace.OrderService.Application.Cart.CheckoutCalculator;

namespace HiveSpace.OrderService.Application.Cart.Queries.GetCartSummary;

public class GetCartSummaryQueryHandler(
    ICartDataQuery cartDataQuery,
    ICheckoutQuery checkoutQuery,
    ICouponRepository couponRepository,
    ICartRepository cartRepository,
    IUserContext userContext)
    : IQueryHandler<GetCartSummaryQuery, GetCartSummaryResponse>
{
    public async Task<GetCartSummaryResponse> Handle(GetCartSummaryQuery request, CancellationToken cancellationToken)
    {
        var userId = userContext.UserId;
        var cart = await cartRepository.GetByUserIdAsync(userId, cancellationToken);
        var pagedResult = await cartDataQuery.GetPagedCartItemsAsync(
            userId,
            request.Page,
            request.PageSize,
            cancellationToken);

        var selectedCart = await checkoutQuery.GetSelectedCartItemsAsync(userId, cancellationToken);
        var snapshots = selectedCart.CartExists && selectedCart.Rows.Length > 0
            ? SelectedCartCouponEvaluator.BuildStoreSnapshots(selectedCart)
            : [];
        var couponState = cart is not null
            ? await PersistedCartCouponState.ValidateAsync(
                cart,
                snapshots,
                couponRepository,
                userId,
                cancellationToken,
                removeInvalidSelections: true)
            : new CartCouponValidationResult([], [], [], new Dictionary<string, Coupon>(StringComparer.OrdinalIgnoreCase));
        var summary = BuildSummary(snapshots, couponState, userId);
        var discountedItems = ApplyItemDiscounts(
            pagedResult.Items,
            selectedCart,
            snapshots,
            couponState,
            userId);

        if (cart is not null && couponState.InvalidatedCoupons.Count > 0)
            await cartRepository.SaveChangesAsync(cancellationToken);

        var stores = discountedItems
            .GroupBy(x => x.StoreId)
            .Select(group => new CartStoreGroupDto(
                group.Key,
                group.First().StoreName ?? string.Empty,
                group.First().StoreStatus,
                false,
                group.All(x => x.IsSelected),
                couponState.AppliedStoreCoupons.GetValueOrDefault(group.Key),
                group.ToList()))
            .ToList();
        var hasMore = pagedResult.Pagination.HasNextPage;

        return new GetCartSummaryResponse(
            stores,
            summary,
            couponState.AppliedPlatformCoupons,
            couponState.InvalidatedCoupons,
            hasMore);
    }

    private static List<CartItemDto> ApplyItemDiscounts(
        IReadOnlyCollection<CartItemDto> items,
        CheckoutPreviewRawResult selectedCart,
        IReadOnlyCollection<SelectedCartStoreSnapshot> snapshots,
        CartCouponValidationResult couponState,
        Guid userId)
    {
        if (items.Count == 0)
            return [];

        var grandSelectedSubtotal = snapshots.Sum(x => x.Subtotal);
        var coupons = couponState.CouponsByCode.Values.ToList();
        var platformDiscount = CalculatePlatformDiscount(
            couponState.AppliedPlatformCoupons.Select(x => x.CouponCode).ToList(),
            coupons,
            userId,
            grandSelectedSubtotal);
        var selectedItemIds = selectedCart.Rows
            .Select(x => x.CartItemId)
            .ToHashSet();
        var snapshotsByStore = snapshots.ToDictionary(x => x.StoreId);

        return items
            .GroupBy(x => x.StoreId)
            .SelectMany(group =>
            {
                if (!snapshotsByStore.TryGetValue(group.Key, out var snapshot) || snapshot.Subtotal <= 0)
                {
                    return group.Select(item => item with
                    {
                        OriginalPrice = item.OriginalPrice ?? item.Price,
                        Price = item.OriginalPrice ?? item.Price
                    });
                }

                var storeCouponEvaluation = GetStoreCouponEvaluation(group.Key, snapshot, couponState, coupons, userId);
                var storePlatformDiscount = grandSelectedSubtotal > 0
                    ? platformDiscount * snapshot.Subtotal / grandSelectedSubtotal
                    : 0L;

                return group.Select(item =>
                {
                    var originalPrice = item.OriginalPrice ?? item.Price ?? 0L;
                    var discountedPrice = originalPrice;

                    if (item.IsSelected && selectedItemIds.Contains(item.CartItemId))
                    {
                        if (storeCouponEvaluation.ItemDiscount > 0 &&
                            storeCouponEvaluation.EligibleSubtotal > 0 &&
                            storeCouponEvaluation.EligibleProductIds.Contains(item.ProductId))
                        {
                            discountedPrice -= originalPrice * storeCouponEvaluation.ItemDiscount / storeCouponEvaluation.EligibleSubtotal;
                        }

                        if (storePlatformDiscount > 0 && snapshot.Subtotal > 0)
                            discountedPrice -= originalPrice * storePlatformDiscount / snapshot.Subtotal;

                        discountedPrice = Math.Max(0L, discountedPrice);
                    }

                    return item with
                    {
                        OriginalPrice = originalPrice,
                        Price = discountedPrice
                    };
                });
            })
            .ToList();
    }

    private static CartSummaryTotalsResponse BuildSummary(
        IReadOnlyCollection<SelectedCartStoreSnapshot> snapshots,
        CartCouponValidationResult couponState,
        Guid userId)
    {
        if (snapshots.Count == 0)
            return new CartSummaryTotalsResponse(0L, 0L, 0L);

        var grandSubTotal = snapshots.Sum(x => x.Subtotal);
        var grandShipping = snapshots.Sum(x => x.ShippingFee);

        var coupons = couponState.CouponsByCode.Values.ToList();
        var storeDiscount = snapshots.Sum(snapshot => GetStoreDiscount(snapshot.StoreId, snapshot, couponState, coupons, userId));

        var platformDiscount = CalculatePlatformDiscount(
            couponState.AppliedPlatformCoupons.Select(x => x.CouponCode).ToList(),
            coupons,
            userId,
            grandSubTotal);

        var discountAmount = storeDiscount + platformDiscount;
        return new CartSummaryTotalsResponse(
            DiscountAmount: discountAmount,
            SubTotal: grandSubTotal,
            Total: Math.Max(0L, grandSubTotal + grandShipping - discountAmount));
    }

    private static long GetStoreDiscount(
        Guid storeId,
        SelectedCartStoreSnapshot snapshot,
        CartCouponValidationResult couponState,
        List<Coupon> coupons,
        Guid userId)
        => GetStoreCouponEvaluation(storeId, snapshot, couponState, coupons, userId).DiscountAmount;

    private static long CalculatePlatformDiscount(
        List<string> codes,
        List<Coupon> coupons,
        Guid userId,
        long totalSubTotal)
    {
        if (codes.Count == 0)
            return 0L;

        var total = 0L;
        foreach (var code in codes)
        {
            var coupon = coupons.FirstOrDefault(c =>
                c.Code.Equals(code, StringComparison.OrdinalIgnoreCase) &&
                c.OwnerType == CouponOwnerType.Platform);
            if (coupon is null)
                continue;

            var (itemDiscount, _) = ApplyCoupon(coupon, userId, totalSubTotal, shippingFee: 0);
            total += itemDiscount;
        }

        return total;
    }

    private static CouponEvaluationResult GetStoreCouponEvaluation(
        Guid storeId,
        SelectedCartStoreSnapshot snapshot,
        CartCouponValidationResult couponState,
        List<Coupon> coupons,
        Guid userId)
    {
        if (!couponState.AppliedStoreCoupons.TryGetValue(storeId, out var appliedStoreCoupon))
            return new CouponEvaluationResult(false, 0L, 0L, 0L, 0L, [], []);

        var coupon = coupons.FirstOrDefault(c =>
            c.Code.Equals(appliedStoreCoupon.CouponCode, StringComparison.OrdinalIgnoreCase));

        return coupon is null
            ? new CouponEvaluationResult(false, 0L, 0L, 0L, 0L, [], [])
            : SelectedCartCouponEvaluator.EvaluateCoupon(coupon, userId, snapshot);
    }
}
