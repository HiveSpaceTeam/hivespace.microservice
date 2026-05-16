using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.Domain.Shared.ValueObjects;
using HiveSpace.OrderService.Application.Cart.Dtos;
using HiveSpace.OrderService.Domain.Aggregates.Carts;
using HiveSpace.OrderService.Domain.Aggregates.Coupons;
using HiveSpace.OrderService.Domain.External;
using HiveSpace.OrderService.Domain.Exceptions;
using static HiveSpace.OrderService.Application.Cart.CheckoutCalculator;

namespace HiveSpace.OrderService.Application.Cart;

public static class SelectedCartCouponEvaluator
{
    public static List<SelectedCartStoreSnapshot> BuildStoreSnapshots(
        IReadOnlyCollection<CartItem> items,
        IReadOnlyDictionary<long, ProductRef> productsById,
        IReadOnlyDictionary<long, SkuRef> skusById)
    {
        var selectedItems = items
            .Where(x => x.IsSelected)
            .Select(x => new
            {
                Item = x,
                Product = productsById.GetValueOrDefault(x.ProductId),
                Sku = skusById.GetValueOrDefault(x.SkuId)
            })
            .Where(x => x.Product is not null && x.Sku is not null)
            .ToList();

        if (selectedItems.Count == 0)
            return [];

        var totalItemCount = selectedItems.Sum(x => x.Item.Quantity);
        var rawShippingFee = CalculateShippingFee(totalItemCount);
        var storeGroups = selectedItems.GroupBy(x => x.Product!.StoreId).ToList();
        var shippingPerStore = DistributeShippingFee(rawShippingFee, storeGroups.Count);

        var snapshots = new List<SelectedCartStoreSnapshot>(storeGroups.Count);
        for (int i = 0; i < storeGroups.Count; i++)
        {
            var group = storeGroups[i];
            var currency = group.First().Sku!.Currency;

            snapshots.Add(new SelectedCartStoreSnapshot(
                group.Key,
                StoreName: null,
                currency,
                group.Sum(x => x.Sku!.Price * x.Item.Quantity),
                shippingPerStore[i],
                group.Select(x => x.Item.ProductId).Distinct().ToList(),
                group.Select(x => new SelectedCartStoreLineSnapshot(
                    x.Item.ProductId,
                    x.Sku!.Price * x.Item.Quantity))
                    .ToList()
            ));
        }

        return snapshots;
    }

    public static void EnsureSelectedCartExists(CheckoutPreviewRawResult result, string source)
    {
        if (!result.CartExists)
            throw new NotFoundException(OrderDomainErrorCode.CartNotFound, source);

        if (result.Rows.Length == 0)
            throw new InvalidFieldException(OrderDomainErrorCode.CartEmpty, source);
    }

    public static List<SelectedCartStoreSnapshot> BuildStoreSnapshots(CheckoutPreviewRawResult result)
    {
        EnsureSelectedCartExists(result, nameof(SelectedCartCouponEvaluator));

        var totalItemCount = result.Rows.Sum(r => r.Quantity);
        var rawShippingFee = CalculateShippingFee(totalItemCount);
        var currency = result.Rows.FirstOrDefault(r => r.Currency != null)?.Currency ?? "VND";

        var storeGroups = result.Rows.GroupBy(r => r.StoreId).ToList();
        var shippingPerStore = DistributeShippingFee(rawShippingFee, storeGroups.Count);

        var snapshots = new List<SelectedCartStoreSnapshot>(storeGroups.Count);
        for (int i = 0; i < storeGroups.Count; i++)
        {
            var group = storeGroups[i];
            snapshots.Add(new SelectedCartStoreSnapshot(
                group.Key,
                group.First().StoreName,
                currency,
                group.Sum(r => (r.Price ?? 0L) * r.Quantity),
                shippingPerStore[i],
                group.Select(r => r.ProductId).Distinct().ToList(),
                group.Select(r => new SelectedCartStoreLineSnapshot(
                    r.ProductId,
                    (r.Price ?? 0L) * r.Quantity))
                    .ToList()
            ));
        }

        return snapshots;
    }

    public static SelectedCartStoreSnapshot GetStoreSnapshot(
        CheckoutPreviewRawResult result,
        Guid storeId,
        string source)
    {
        EnsureSelectedCartExists(result, source);

        var snapshot = BuildStoreSnapshots(result)
            .FirstOrDefault(x => x.StoreId == storeId);

        return snapshot
            ?? throw new InvalidFieldException(OrderDomainErrorCode.CartEmpty, source);
    }

    public static SelectedCartStoreSnapshot FilterSnapshotByProductIds(
        SelectedCartStoreSnapshot snapshot,
        IReadOnlyCollection<long>? productIds)
    {
        if (productIds is null || productIds.Count == 0)
            return snapshot;

        var productIdSet = productIds.ToHashSet();
        var filteredLines = snapshot.Lines
            .Where(x => productIdSet.Contains(x.ProductId))
            .ToList();
        var filteredProductIds = filteredLines
            .Select(x => x.ProductId)
            .Distinct()
            .ToList();

        return snapshot with
        {
            Subtotal = filteredLines.Sum(x => x.LineSubtotal),
            ProductIds = filteredProductIds,
            Lines = filteredLines
        };
    }

    public static CouponEvaluationResult EvaluateCoupon(
        Coupon coupon,
        Guid userId,
        SelectedCartStoreSnapshot snapshot)
    {
        var eligibleProductIds = ResolveEligibleProductIds(coupon, snapshot);
        var eligibleSubtotal = ResolveEligibleSubtotal(coupon, snapshot, eligibleProductIds);

        if (coupon.ApplicableProductIds.Count > 0 && eligibleProductIds.Count == 0)
        {
            return new CouponEvaluationResult(
                false,
                0L,
                0L,
                0L,
                0L,
                [],
                [OrderDomainErrorCode.CouponProductNotApplicable]);
        }

        var validationSubtotal = coupon.ApplicableProductIds.Count > 0 ? eligibleSubtotal : snapshot.Subtotal;
        var validationProductIds = coupon.ApplicableProductIds.Count > 0
            ? eligibleProductIds
            : snapshot.ProductIds;
        var validation = coupon.Validate(
            userId,
            Money.FromVND(validationSubtotal),
            validationProductIds,
            snapshot.StoreId);

        if (!validation.IsValid)
        {
            return new CouponEvaluationResult(
                false,
                0L,
                0L,
                0L,
                eligibleSubtotal,
                eligibleProductIds,
                validation.Errors.Select(x => x.ErrorCode).ToList());
        }

        var (itemDiscount, shippingDiscount) = ApplyCoupon(
            coupon,
            userId,
            validationSubtotal,
            snapshot.ShippingFee,
            validationProductIds,
            snapshot.StoreId);

        return new CouponEvaluationResult(
            true,
            itemDiscount + shippingDiscount,
            itemDiscount,
            shippingDiscount,
            eligibleSubtotal,
            eligibleProductIds,
            []);
    }

    private static List<long> ResolveEligibleProductIds(Coupon coupon, SelectedCartStoreSnapshot snapshot)
    {
        if (coupon.ApplicableProductIds.Count == 0)
            return snapshot.ProductIds.Distinct().ToList();

        var applicableProductIds = coupon.ApplicableProductIds.ToHashSet();
        return snapshot.ProductIds
            .Where(applicableProductIds.Contains)
            .Distinct()
            .ToList();
    }

    private static long ResolveEligibleSubtotal(
        Coupon coupon,
        SelectedCartStoreSnapshot snapshot,
        IReadOnlyCollection<long> eligibleProductIds)
    {
        if (coupon.ApplicableProductIds.Count == 0)
            return snapshot.Subtotal;

        if (eligibleProductIds.Count == 0)
            return 0L;

        var eligibleProductIdSet = eligibleProductIds.ToHashSet();
        return snapshot.Lines
            .Where(x => eligibleProductIdSet.Contains(x.ProductId))
            .Sum(x => x.LineSubtotal);
    }
}
