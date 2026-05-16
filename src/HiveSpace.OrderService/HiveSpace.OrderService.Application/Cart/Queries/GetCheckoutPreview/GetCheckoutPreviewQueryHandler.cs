using HiveSpace.Application.Shared.Handlers;
using HiveSpace.Core.Contexts;
using HiveSpace.OrderService.Application.Cart.Dtos;
using HiveSpace.OrderService.Domain.Aggregates.Carts;
using HiveSpace.OrderService.Domain.Aggregates.Coupons;
using HiveSpace.OrderService.Domain.Enumerations;
using HiveSpace.OrderService.Domain.Repositories;
using static HiveSpace.OrderService.Application.Cart.CheckoutCalculator;

namespace HiveSpace.OrderService.Application.Cart.Queries.GetCheckoutPreview;

public class GetCheckoutPreviewQueryHandler(
    ICheckoutQuery checkoutQuery,
    ICouponRepository couponRepository,
    ICartRepository cartRepository,
    IUserContext userContext)
    : IQueryHandler<GetCheckoutPreviewQuery, CheckoutPreviewResponse>
{
    public async Task<CheckoutPreviewResponse> Handle(
        GetCheckoutPreviewQuery request, CancellationToken cancellationToken)
    {
        var userId = userContext.UserId;
        var cart = await cartRepository.GetByUserIdAsync(userId, cancellationToken)
            ?? throw new HiveSpace.Domain.Shared.Exceptions.NotFoundException(HiveSpace.OrderService.Domain.Exceptions.OrderDomainErrorCode.CartNotFound, nameof(Cart));

        var result = await checkoutQuery.GetSelectedCartItemsAsync(userId, cancellationToken);
        SelectedCartCouponEvaluator.EnsureSelectedCartExists(result, nameof(GetCheckoutPreviewQueryHandler));

        var totalItemCount = result.Rows.Sum(r => r.Quantity);
        var rawShippingFee = CalculateShippingFee(totalItemCount);
        var currency = result.Rows.FirstOrDefault(r => r.Currency != null)?.Currency ?? "VND";
        var snapshots = SelectedCartCouponEvaluator.BuildStoreSnapshots(result);
        var couponState = await PersistedCartCouponState.ValidateAsync(
            cart,
            snapshots,
            couponRepository,
            userId,
            cancellationToken,
            removeInvalidSelections: true);

        if (couponState.InvalidatedCoupons.Count > 0)
            await cartRepository.SaveChangesAsync(cancellationToken);

        var coupons = couponState.CouponsByCode.Values.ToList();
        var storeGroups = result.Rows.GroupBy(r => r.StoreId).ToList();
        var shippingPerStore = DistributeShippingFee(rawShippingFee, storeGroups.Count);
        var grandOriginalSubtotal = result.Rows.Sum(r => (r.Price ?? 0L) * r.Quantity);
        var platformDiscount = CalculatePlatformDiscount(
            couponState.AppliedPlatformCoupons.Select(x => x.CouponCode).ToList(),
            coupons,
            userId,
            grandOriginalSubtotal);

        var packages = new List<CheckoutPreviewPackageDto>();
        for (int i = 0; i < storeGroups.Count; i++)
        {
            var group = storeGroups[i];
            var pkgOriginalShipping = shippingPerStore[i];
            var pkgOriginalSubtotal = group.Sum(r => (r.Price ?? 0L) * r.Quantity);
            var productIds = group.Select(r => r.ProductId).ToList();

            var snapshot = new SelectedCartStoreSnapshot(
                group.Key,
                group.First().StoreName,
                currency,
                pkgOriginalSubtotal,
                pkgOriginalShipping,
                productIds.Distinct().ToList(),
                group.Select(r => new SelectedCartStoreLineSnapshot(
                    r.ProductId,
                    (r.Price ?? 0L) * r.Quantity))
                    .ToList());

            var appliedStoreCoupon = couponState.AppliedStoreCoupons.GetValueOrDefault(group.Key);
            var storeCoupon = appliedStoreCoupon is null
                ? null
                : coupons.FirstOrDefault(c => c.Code.Equals(appliedStoreCoupon.CouponCode, StringComparison.OrdinalIgnoreCase));
            var couponEvaluation = storeCoupon is not null
                ? SelectedCartCouponEvaluator.EvaluateCoupon(storeCoupon, userId, snapshot)
                : new CouponEvaluationResult(false, 0L, 0L, 0L, 0L, [], []);

            var pkgShippingDiscount = couponEvaluation.ShippingDiscount;
            var pkgPlatformDiscount = grandOriginalSubtotal > 0
                ? platformDiscount * pkgOriginalSubtotal / grandOriginalSubtotal
                : 0L;

            var items = group.Select(r =>
            {
                var originalPrice = r.Price ?? 0L;
                var discountedPrice = originalPrice;

                if (couponEvaluation.ItemDiscount > 0 &&
                    couponEvaluation.EligibleSubtotal > 0 &&
                    couponEvaluation.EligibleProductIds.Contains(r.ProductId))
                {
                    discountedPrice -= originalPrice * couponEvaluation.ItemDiscount / couponEvaluation.EligibleSubtotal;
                }

                if (pkgPlatformDiscount > 0 && pkgOriginalSubtotal > 0)
                    discountedPrice -= originalPrice * pkgPlatformDiscount / pkgOriginalSubtotal;

                discountedPrice = Math.Max(0L, discountedPrice);

                return new CheckoutPreviewItemDto(
                    CartItemId: r.CartItemId,
                    ProductId: r.ProductId,
                    SkuId: r.SkuId,
                    ProductName: r.ProductName,
                    ImageUrl: r.SkuImageUrl ?? r.ThumbnailUrl,
                    SkuName: r.SkuName,
                    SkuAttributes: r.SkuAttributes,
                    OriginalPrice: originalPrice,
                    Price: discountedPrice,
                    Currency: r.Currency ?? currency,
                    Quantity: r.Quantity,
                    LineTotal: discountedPrice * r.Quantity
                );
            }).ToList();

            var pkgSubtotal = items.Sum(it => it.LineTotal);
            var pkgShippingFee = Math.Max(0L, pkgOriginalShipping - pkgShippingDiscount);

            packages.Add(new CheckoutPreviewPackageDto(
                StoreId: group.Key,
                StoreName: group.First().StoreName,
                OriginalShippingFee: pkgOriginalShipping,
                ShippingFee: pkgShippingFee,
                ShippingType: "economy",
                Currency: currency,
                OriginalSubtotal: pkgOriginalSubtotal,
                Subtotal: pkgSubtotal,
                PackageTotal: pkgSubtotal + pkgShippingFee,
                AppliedStoreCoupon: appliedStoreCoupon,
                Items: items
            ));
        }

        var grandSubtotal = packages.Sum(p => p.Subtotal);
        var grandShipping = packages.Sum(p => p.ShippingFee);

        return new CheckoutPreviewResponse(
            Packages: packages,
            OriginalSubtotal: grandOriginalSubtotal,
            Subtotal: grandSubtotal,
            Currency: currency,
            TotalShippingFee: grandShipping,
            GrandTotal: grandSubtotal + grandShipping,
            TotalItems: totalItemCount,
            PlatformCoupons: couponState.AppliedPlatformCoupons,
            InvalidatedCoupons: couponState.InvalidatedCoupons
        );
    }

    private static long CalculatePlatformDiscount(
        List<string> codes, List<Coupon> coupons, Guid userId, long totalSubtotal)
    {
        if (codes.Count == 0) return 0L;

        var total = 0L;
        foreach (var code in codes)
        {
            var coupon = coupons.FirstOrDefault(c =>
                c.Code.Equals(code, StringComparison.OrdinalIgnoreCase) &&
                c.OwnerType == CouponOwnerType.Platform);
            if (coupon is null) continue;

            var (itemDiscount, _) = ApplyCoupon(coupon, userId, totalSubtotal, shippingFee: 0);
            total += itemDiscount;
        }
        return total;
    }
}
