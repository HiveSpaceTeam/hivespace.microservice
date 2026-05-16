using HiveSpace.Domain.Shared.Enumerations;
using HiveSpace.Domain.Shared.ValueObjects;
using HiveSpace.Infrastructure.Persistence.Seeding;
using HiveSpace.OrderService.Domain.Aggregates.Coupons;
using HiveSpace.OrderService.Domain.Enumerations;
using HiveSpace.OrderService.Infrastructure.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HiveSpace.OrderService.Infrastructure.SeedData;

internal sealed class CouponSeeder(OrderDbContext db, ILogger<CouponSeeder> logger) : ISeeder
{
    private const int SqlUniqueIndexViolationErrorCode = 2601;
    private const int SqlUniqueConstraintViolationErrorCode = 2627;

    public int Order => 3;

    private static readonly StoreCouponSeed[] Stores =
    [
        new(
            new Guid("b2c3d4e5-f6a7-8901-bcde-f12345678901"),
            new Guid("a1b2c3d4-e5f6-7890-abcd-ef1234567890"),
            "TK",
            new ProductSelection(1011L, [1012L, 1013L], [1011L, 1012L])),
        new(
            new Guid("e5f6a7b8-c9d0-1234-ef01-234567890123"),
            new Guid("c3d4e5f6-a7b8-9012-cdef-012345678901"),
            "GV",
            new ProductSelection(1001L, [1002L, 1005L], [1001L, 1002L])),
        new(
            new Guid("f6a7b8c9-d0e1-2345-f012-345678901234"),
            new Guid("d4e5f6a7-b8c9-0123-def0-123456789012"),
            "PD",
            new ProductSelection(1003L, [1004L, 1007L], [1003L, 1004L])),
    ];

    public async Task SeedAsync(CancellationToken ct = default)
    {
        var now = DateTimeOffset.UtcNow;

        var templates = Stores.SelectMany((store, idx) => BuildTemplates(now, store, idx)).ToList();

        var strategy = db.Database.CreateExecutionStrategy();
        try
        {
            await strategy.ExecuteAsync(async () =>
            {
                var expectedCodes = templates.Select(t => t.Code).ToList();

                db.ChangeTracker.Clear();
                var existingCodes = await db.Coupons
                    .Where(c => expectedCodes.Contains(c.Code))
                    .Select(c => c.Code)
                    .ToListAsync(ct);
                var existingCodeSet = existingCodes.ToHashSet();

                var toInsert = templates.Where(t => !existingCodeSet.Contains(t.Code)).ToList();
                if (toInsert.Count == 0)
                {
                    logger.LogDebug("All expected Coupons already exist. Skipping.");
                    return;
                }

                await using var tx = await db.Database.BeginTransactionAsync(ct);

                foreach (var t in toInsert)
                {
                    var discountAmount    = t.FixedAmountCents.HasValue ? Money.Create(t.FixedAmountCents.Value, Currency.VND.GetCode()) : null;
                    var maxDiscountAmount = t.MaxDiscountCents.HasValue ? Money.Create(t.MaxDiscountCents.Value, Currency.VND.GetCode()) : null;
                    var minOrderAmount    = Money.Create(t.MinOrderCents, Currency.VND.GetCode());

                    var coupon = Coupon.CreateByStore(
                        storeId: t.StoreId,
                        storeOwnerId: t.OwnerId,
                        code: t.Code,
                        name: t.Name,
                        discountType: t.DiscountType,
                        percentage: t.Percentage,
                        discountAmount: discountAmount,
                        scope: t.Scope,
                        startDateTime: t.StartDateTime,
                        endDateTime: t.EndDateTime,
                        earlySaveDateTime: null,
                        isHidden: t.IsHidden,
                        maxDiscountAmount: maxDiscountAmount,
                        minOrderAmount: minOrderAmount,
                        id: t.Id);

                    coupon.SetMaxUsageCount(t.MaxUsageCount);
                    coupon.SetMaxUsagePerUser(t.MaxUsagePerUser);
                    if (t.ProductIds.Length > 0)
                        coupon.LimitToProducts(t.ProductIds);

                    db.Coupons.Add(coupon);
                }

                await db.SaveChangesAsync(ct);
                await tx.CommitAsync(ct);
                logger.LogInformation("Seeded Coupon(s) via CouponSeeder.");
            });
        }
        catch (DbUpdateException ex) when (IsDuplicateKeyException(ex))
        {
            logger.LogInformation("Coupon seeding skipped because coupons were inserted concurrently by another instance.");
        }
    }

    private static bool IsDuplicateKeyException(DbUpdateException ex)
    {
        var current = ex.InnerException;
        while (current is not null)
        {
            if (current is SqlException sqlEx &&
                (sqlEx.Number is SqlUniqueIndexViolationErrorCode or SqlUniqueConstraintViolationErrorCode))
                return true;

            current = current.InnerException;
        }

        return false;
    }

    private static IEnumerable<CouponTemplate> BuildTemplates(DateTimeOffset now, StoreCouponSeed store, int idx) =>
    [
        // Entire shop
        new(CreateTemplateId(idx, 1),  $"{store.Prefix}ESOGP01", $"[{store.Prefix}] Entire Shop 12% Off - Ongoing",
            DiscountType.Percentage, 12m, null, CouponScope.ItemPrice,
            now.AddDays(-14).AddHours(2 + idx), now.AddDays(18).AddHours(6 + idx), false, 9000L, 45000L, 1000, 3, [], store.StoreId, store.OwnerId),
        new(CreateTemplateId(idx, 2),  $"{store.Prefix}ESOGF01", $"[{store.Prefix}] Entire Shop 3,500 VND Off - Ongoing",
            DiscountType.FixedAmount, null, 3500L, CouponScope.ItemPrice,
            now.AddDays(-14).AddHours(4 + idx), now.AddDays(18).AddHours(8 + idx), false, null, 18000L, 500, 2, [], store.StoreId, store.OwnerId),
        new(CreateTemplateId(idx, 3),  $"{store.Prefix}ESUPP01", $"[{store.Prefix}] Entire Shop 10% Off - Upcoming",
            DiscountType.Percentage, 10m, null, CouponScope.ItemPrice,
            now.AddDays(6).AddHours(2 + idx), now.AddDays(34).AddHours(6 + idx), false, 7000L, 32000L, 750, 2, [], store.StoreId, store.OwnerId),
        new(CreateTemplateId(idx, 4),  $"{store.Prefix}ESUPF01", $"[{store.Prefix}] Entire Shop 4,200 VND Off - Upcoming",
            DiscountType.FixedAmount, null, 4200L, CouponScope.ItemPrice,
            now.AddDays(6).AddHours(4 + idx), now.AddDays(34).AddHours(8 + idx), false, null, 22000L, 500, 2, [], store.StoreId, store.OwnerId),
        new(CreateTemplateId(idx, 5),  $"{store.Prefix}ESEXPP01", $"[{store.Prefix}] Entire Shop 9% Shipping Off - Expired",
            DiscountType.Percentage, 9m, null, CouponScope.ShippingFee,
            now.AddDays(-36).AddHours(1 + idx), now.AddDays(-4).AddHours(5 + idx), false, 1800L, 18000L, 100, 1, [], store.StoreId, store.OwnerId),
        new(CreateTemplateId(idx, 6),  $"{store.Prefix}ESEXPF01", $"[{store.Prefix}] Entire Shop 1,200 VND Shipping Off - Expired",
            DiscountType.FixedAmount, null, 1200L, CouponScope.ShippingFee,
            now.AddDays(-36).AddHours(3 + idx), now.AddDays(-4).AddHours(7 + idx), false, null, 6500L, 100, 1, [], store.StoreId, store.OwnerId),

        // Specific products
        new(CreateTemplateId(idx, 7),  $"{store.Prefix}SPOGP01", $"[{store.Prefix}] Products 18% Off - Ongoing",
            DiscountType.Percentage, 18m, null, CouponScope.ItemPrice,
            now.AddDays(-9).AddHours(3 + idx), now.AddDays(24).AddHours(7 + idx), false, 7200L, 28000L, 200, 2, [store.Products.OngoingProductId], store.StoreId, store.OwnerId),
        new(CreateTemplateId(idx, 8),  $"{store.Prefix}SPOGF01", $"[{store.Prefix}] Products 3,800 VND Off - Ongoing",
            DiscountType.FixedAmount, null, 3800L, CouponScope.ItemPrice,
            now.AddDays(-9).AddHours(5 + idx), now.AddDays(24).AddHours(9 + idx), false, null, 19000L, 160, 2, [store.Products.OngoingProductId], store.StoreId, store.OwnerId),
        new(CreateTemplateId(idx, 9),  $"{store.Prefix}SPUPP01", $"[{store.Prefix}] Products 14% Off - Upcoming",
            DiscountType.Percentage, 14m, null, CouponScope.ItemPrice,
            now.AddDays(9).AddHours(2 + idx), now.AddDays(38).AddHours(8 + idx), false, 6800L, 24000L, 150, 1, store.Products.UpcomingProductIds, store.StoreId, store.OwnerId),
        new(CreateTemplateId(idx, 10), $"{store.Prefix}SPUPF01", $"[{store.Prefix}] Products 4,200 VND Off - Upcoming",
            DiscountType.FixedAmount, null, 4200L, CouponScope.ItemPrice,
            now.AddDays(9).AddHours(4 + idx), now.AddDays(38).AddHours(10 + idx), false, null, 22000L, 150, 1, store.Products.UpcomingProductIds, store.StoreId, store.OwnerId),
        new(CreateTemplateId(idx, 11), $"{store.Prefix}SPEXPP01", $"[{store.Prefix}] Products 12% Shipping Off - Expired",
            DiscountType.Percentage, 12m, null, CouponScope.ShippingFee,
            now.AddDays(-48).AddHours(6 + idx), now.AddDays(-12).AddHours(2 + idx), false, 1500L, 9000L, 50, 1, store.Products.ExpiredProductIds, store.StoreId, store.OwnerId),
        new(CreateTemplateId(idx, 12), $"{store.Prefix}SPEXPF01", $"[{store.Prefix}] Products 1,000 VND Shipping Off - Expired",
            DiscountType.FixedAmount, null, 1000L, CouponScope.ShippingFee,
            now.AddDays(-48).AddHours(8 + idx), now.AddDays(-12).AddHours(4 + idx), false, null, 5000L, 50, 1, store.Products.ExpiredProductIds, store.StoreId, store.OwnerId),

        // Private
        new(CreateTemplateId(idx, 13), $"{store.Prefix}PVOGP01", $"[{store.Prefix}] Private 7% Off - Ongoing",
            DiscountType.Percentage, 7m, null, CouponScope.ItemPrice,
            now.AddDays(-7).AddHours(5 + idx), now.AddDays(16).AddHours(9 + idx), true, 5000L, 30000L, 2000, 10, [], store.StoreId, store.OwnerId),
        new(CreateTemplateId(idx, 14), $"{store.Prefix}PVOGF01", $"[{store.Prefix}] Private 4,800 VND Off - Ongoing",
            DiscountType.FixedAmount, null, 4800L, CouponScope.ItemPrice,
            now.AddDays(-7).AddHours(7 + idx), now.AddDays(16).AddHours(11 + idx), true, null, 21000L, 1200, 6, [], store.StoreId, store.OwnerId),
        new(CreateTemplateId(idx, 15), $"{store.Prefix}PVUPP01", $"[{store.Prefix}] Private 8% Off - Upcoming",
            DiscountType.Percentage, 8m, null, CouponScope.ItemPrice,
            now.AddDays(12).AddHours(1 + idx), now.AddDays(46).AddHours(4 + idx), true, 4200L, 26000L, 300, 5, [], store.StoreId, store.OwnerId),
        new(CreateTemplateId(idx, 16), $"{store.Prefix}PVUPF01", $"[{store.Prefix}] Private 5,600 VND Off - Upcoming",
            DiscountType.FixedAmount, null, 5600L, CouponScope.ItemPrice,
            now.AddDays(12).AddHours(3 + idx), now.AddDays(46).AddHours(6 + idx), true, null, 26000L, 300, 5, [], store.StoreId, store.OwnerId),
        new(CreateTemplateId(idx, 17), $"{store.Prefix}PVEXPP01", $"[{store.Prefix}] Private 11% Off - Expired",
            DiscountType.Percentage, 11m, null, CouponScope.ItemPrice,
            now.AddDays(-30).AddHours(8 + idx), now.AddDays(-3).AddHours(3 + idx), true, 4000L, 24000L, 100, 1, [], store.StoreId, store.OwnerId),
        new(CreateTemplateId(idx, 18), $"{store.Prefix}PVEXPF01", $"[{store.Prefix}] Private 4,400 VND Off - Expired",
            DiscountType.FixedAmount, null, 4400L, CouponScope.ItemPrice,
            now.AddDays(-30).AddHours(10 + idx), now.AddDays(-3).AddHours(5 + idx), true, null, 20000L, 100, 1, [], store.StoreId, store.OwnerId),
    ];

    private static Guid CreateTemplateId(int storeIndex, int templateIndex) =>
        Guid.Parse($"5C86B36B-6B10-4E9F-BF0E-9D66D7{storeIndex:X2}{templateIndex:X4}");

    private sealed record StoreCouponSeed(
        Guid StoreId,
        Guid OwnerId,
        string Prefix,
        ProductSelection Products);

    private sealed record ProductSelection(
        long OngoingProductId,
        long[] UpcomingProductIds,
        long[] ExpiredProductIds);

    private sealed record CouponTemplate(
        Guid         Id,
        string       Code,
        string       Name,
        DiscountType DiscountType,
        decimal?     Percentage,
        long?        FixedAmountCents,
        CouponScope  Scope,
        DateTimeOffset StartDateTime,
        DateTimeOffset EndDateTime,
        bool         IsHidden,
        long?        MaxDiscountCents,
        long         MinOrderCents,
        int          MaxUsageCount,
        int          MaxUsagePerUser,
        long[]       ProductIds,
        Guid         StoreId,
        Guid         OwnerId);
}
