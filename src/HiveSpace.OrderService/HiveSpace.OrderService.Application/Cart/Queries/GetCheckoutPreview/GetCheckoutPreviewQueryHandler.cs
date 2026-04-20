using HiveSpace.Application.Shared.Handlers;
using HiveSpace.Core.Contexts;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.OrderService.Application.Cart.Dtos;
using HiveSpace.OrderService.Domain.Aggregates.Coupons;
using HiveSpace.OrderService.Domain.Enumerations;
using HiveSpace.OrderService.Domain.Exceptions;
using HiveSpace.OrderService.Domain.Repositories;
using static HiveSpace.OrderService.Application.Cart.CheckoutCalculator;

namespace HiveSpace.OrderService.Application.Cart.Queries.GetCheckoutPreview;

public class GetCheckoutPreviewQueryHandler(
    ICheckoutQuery checkoutQuery,
    ICouponRepository couponRepository,
    IUserContext userContext)
    : IQueryHandler<GetCheckoutPreviewQuery, CheckoutPreviewResponse>
{
    public async Task<CheckoutPreviewResponse> Handle(
        GetCheckoutPreviewQuery request, CancellationToken cancellationToken)
    {
        var userId = userContext.UserId;

        var result = await checkoutQuery.GetSelectedCartItemsAsync(userId, cancellationToken);

        if (!result.CartExists)
            throw new NotFoundException(OrderDomainErrorCode.CartNotFound,
                nameof(GetCheckoutPreviewQueryHandler));

        if (result.Rows.Length == 0)
            throw new InvalidFieldException(OrderDomainErrorCode.CartEmpty,
                nameof(GetCheckoutPreviewQueryHandler));

        var totalItemCount    = result.Rows.Sum(r => r.Quantity);
        var rawShippingFee    = CalculateShippingFee(totalItemCount);
        var currency          = result.Rows.FirstOrDefault(r => r.Currency != null)?.Currency ?? "VND";

        var allCodes = new List<string>();
        if (request.StoreCoupons != null) allCodes.AddRange(request.StoreCoupons.Select(s => s.CouponCode));
        if (request.PlatformCouponCodes != null) allCodes.AddRange(request.PlatformCouponCodes);

        var coupons = allCodes.Count > 0
            ? await couponRepository.GetByCodesAsync(allCodes, cancellationToken)
            : [];

        var storeGroups           = result.Rows.GroupBy(r => r.StoreId).ToList();
        var shippingPerStore      = DistributeShippingFee(rawShippingFee, storeGroups.Count);
        var grandOriginalSubtotal = result.Rows.Sum(r => (r.Price ?? 0L) * r.Quantity);
        var platformDiscount      = CalculatePlatformDiscount(
            request.PlatformCouponCodes, coupons, userId, grandOriginalSubtotal);

        var packages = new List<CheckoutPreviewPackageDto>();
        for (int i = 0; i < storeGroups.Count; i++)
        {
            var group = storeGroups[i];
            var pkgOriginalShipping = shippingPerStore[i];
            var pkgOriginalSubtotal = group.Sum(r => (r.Price ?? 0L) * r.Quantity);
            var productIds = group.Select(r => r.ProductId).ToList();

            var storeCouponCode = request.StoreCoupons?
                .FirstOrDefault(sc => sc.StoreId == group.Key)?.CouponCode;
            var storeCoupon = storeCouponCode != null
                ? coupons.FirstOrDefault(c => c.Code.Equals(storeCouponCode, StringComparison.OrdinalIgnoreCase))
                : null;

            var (pkgItemDiscount, pkgShippingDiscount) = storeCoupon != null
                ? ApplyCoupon(storeCoupon, userId, pkgOriginalSubtotal, pkgOriginalShipping, productIds, group.Key)
                : (0L, 0L);

            var pkgPlatformDiscount = grandOriginalSubtotal > 0
                ? platformDiscount * pkgOriginalSubtotal / grandOriginalSubtotal
                : 0L;
            pkgItemDiscount += pkgPlatformDiscount;

            var items = group.Select(r =>
            {
                var originalPrice = r.Price ?? 0L;
                var discountedPrice = pkgOriginalSubtotal > 0 && pkgItemDiscount > 0
                    ? originalPrice - (originalPrice * pkgItemDiscount / pkgOriginalSubtotal)
                    : originalPrice;
                discountedPrice = Math.Max(0L, discountedPrice);

                return new CheckoutPreviewItemDto(
                    CartItemId:    r.CartItemId,
                    ProductId:     r.ProductId,
                    SkuId:         r.SkuId,
                    ProductName:   r.ProductName,
                    ImageUrl:      r.SkuImageUrl ?? r.ThumbnailUrl,
                    SkuName:       r.SkuName,
                    SkuAttributes: r.SkuAttributes,
                    OriginalPrice: originalPrice,
                    Price:         discountedPrice,
                    Currency:      r.Currency ?? currency,
                    Quantity:      r.Quantity,
                    LineTotal:     discountedPrice * r.Quantity
                );
            }).ToList();

            var pkgSubtotal = items.Sum(it => it.LineTotal);
            var pkgShippingFee = Math.Max(0L, pkgOriginalShipping - pkgShippingDiscount);

            packages.Add(new CheckoutPreviewPackageDto(
                StoreId:             group.Key,
                StoreName:           group.First().StoreName,
                OriginalShippingFee: pkgOriginalShipping,
                ShippingFee:         pkgShippingFee,
                ShippingType:        "economy",
                Currency:            currency,
                OriginalSubtotal:    pkgOriginalSubtotal,
                Subtotal:            pkgSubtotal,
                PackageTotal:        pkgSubtotal + pkgShippingFee,
                Items:               items
            ));
        }

        var grandSubtotal = packages.Sum(p => p.Subtotal);
        var grandShipping = packages.Sum(p => p.ShippingFee);

        return new CheckoutPreviewResponse(
            Packages:         packages,
            OriginalSubtotal: grandOriginalSubtotal,
            Subtotal:         grandSubtotal,
            Currency:         currency,
            TotalShippingFee: grandShipping,
            GrandTotal:       grandSubtotal + grandShipping,
            TotalItems:       totalItemCount
        );
    }

    private static long CalculatePlatformDiscount(
        List<string>? codes, List<Coupon> coupons, Guid userId, long totalSubtotal)
    {
        if (codes == null || codes.Count == 0) return 0L;

        var total = 0L;
        foreach (var code in codes)
        {
            var coupon = coupons.FirstOrDefault(c =>
                c.Code.Equals(code, StringComparison.OrdinalIgnoreCase) &&
                c.OwnerType == CouponOwnerType.Platform);
            if (coupon == null) continue;

            var (itemDiscount, _) = ApplyCoupon(coupon, userId, totalSubtotal, shippingFee: 0);
            total += itemDiscount;
        }
        return total;
    }
}
