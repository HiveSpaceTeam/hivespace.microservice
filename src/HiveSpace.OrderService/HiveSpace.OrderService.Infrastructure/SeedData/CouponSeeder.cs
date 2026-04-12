using HiveSpace.Domain.Shared.Enumerations;
using HiveSpace.Domain.Shared.ValueObjects;
using HiveSpace.Infrastructure.Persistence.Seeding;
using HiveSpace.OrderService.Domain.Aggregates.Coupons;
using HiveSpace.OrderService.Domain.Enumerations;
using HiveSpace.OrderService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HiveSpace.OrderService.Infrastructure.SeedData;

internal sealed class CouponSeeder(OrderDbContext db, ILogger<CouponSeeder> logger) : ISeeder
{
    public int Order => 3;

    private static readonly (Guid StoreId, Guid OwnerId, string Prefix)[] Stores = 
    [
        (new Guid("b2c3d4e5-f6a7-8901-bcde-f12345678901"), new Guid("a1b2c3d4-e5f6-7890-abcd-ef1234567890"), "TK"),
        (new Guid("e5f6a7b8-c9d0-1234-ef01-234567890123"), new Guid("c3d4e5f6-a7b8-9012-cdef-012345678901"), "GV"),
        (new Guid("f6a7b8c9-d0e1-2345-f012-345678901234"), new Guid("d4e5f6a7-b8c9-0123-def0-123456789012"), "PD"),
    ];

    public async Task SeedAsync(CancellationToken ct = default)
    {
        var now = DateTimeOffset.UtcNow;

        var templates = Stores.SelectMany((store, idx) => BuildTemplates(now, store.StoreId, store.OwnerId, store.Prefix, idx)).ToList();

        var expectedCodes = templates.Select(t => t.Code).ToList();
        var existingCodes = await db.Coupons
            .Where(c => expectedCodes.Contains(c.Code))
            .Select(c => c.Code)
            .ToHashSetAsync(ct);

        var missing = templates.Where(t => !existingCodes.Contains(t.Code)).ToList();
        if (missing.Count == 0)
        {
            logger.LogDebug("All expected Coupons already exist. Skipping.");
            return;
        }

        var strategy = db.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await using var tx = await db.Database.BeginTransactionAsync(ct);

            foreach (var t in missing)
            {
                var discountAmount    = t.FixedAmountCents.HasValue  ? Money.Create(t.FixedAmountCents.Value,  Currency.VND.GetCode()) : null;
                var maxDiscountAmount = t.MaxDiscountCents.HasValue   ? Money.Create(t.MaxDiscountCents.Value,  Currency.VND.GetCode()) : null;
                var minOrderAmount    = Money.Create(t.MinOrderCents, Currency.VND.GetCode());

                var coupon = Coupon.CreateByStore(
                    storeId:          t.StoreId,
                    storeOwnerId:     t.OwnerId,
                    code:             t.Code,
                    name:             t.Name,
                    discountType:     t.DiscountType,
                    percentage:       t.Percentage,
                    discountAmount:   discountAmount,
                    scope:            t.Scope,
                    startDateTime:    t.StartDateTime,
                    endDateTime:      t.EndDateTime,
                    earlySaveDateTime: null,
                    isHidden:         t.IsHidden,
                    maxDiscountAmount: maxDiscountAmount,
                    minOrderAmount:   minOrderAmount,
                    id:               t.Id);

                coupon.SetMaxUsageCount(t.MaxUsageCount);
                coupon.SetMaxUsagePerUser(t.MaxUsagePerUser);
                if (t.ProductIds.Length > 0)
                    coupon.LimitToProducts(t.ProductIds);

                db.Coupons.Add(coupon);
            }

            await db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
        });
        logger.LogInformation("Seeded {Count} Coupon(s).", missing.Count);
    }

    private static IEnumerable<CouponTemplate> BuildTemplates(DateTimeOffset now, Guid storeId, Guid ownerId, string prefix, int idx) =>
    [
        // Entire shop
        new(Guid.Parse($"5C86B36B-6B10-4E9F-BF0E-9D66D7069{idx:D2}1"), $"{prefix}ESONG01",  $"[{prefix}] Entire Shop 10% Off - Ongoing",
            DiscountType.Percentage,  10m,   null,  CouponScope.ItemPrice,
            now.AddDays(-10).AddHours(idx), now.AddDays(20).AddHours(idx),  false, 5000L,  2000L, 1000, 3, [], storeId, ownerId),
        new(Guid.Parse($"5C86B36B-6B10-4E9F-BF0E-9D66D7069{idx:D2}2"), $"{prefix}ESUP01",   $"[{prefix}] Entire Shop $20 Off - Upcoming",
            DiscountType.FixedAmount, null,  2000L, CouponScope.ItemPrice,
            now.AddDays(10).AddHours(idx),  now.AddDays(40).AddHours(idx),  false, null,   10000L, 500,  2, [], storeId, ownerId),
        new(Guid.Parse($"5C86B36B-6B10-4E9F-BF0E-9D66D7069{idx:D2}3"), $"{prefix}ESEXP01",  $"[{prefix}] Entire Shop $5 Shipping - Expired",
            DiscountType.FixedAmount, null,  500L,  CouponScope.ShippingFee,
            now.AddDays(-40).AddHours(idx), now.AddDays(-10).AddHours(idx), false, null,   5000L,  100,  1, [], storeId, ownerId),

        // Specific products
        new(Guid.Parse($"5C86B36B-6B10-4E9F-BF0E-9D66D7069{idx:D2}4"), $"{prefix}SPONG01",  $"[{prefix}] Products 15% Off - Ongoing",
            DiscountType.Percentage,  15m,   null,  CouponScope.ItemPrice,
            now.AddDays(-10).AddHours(idx), now.AddDays(20).AddHours(idx),  false, 2500L,  2000L,  200,  2, [1001L], storeId, ownerId),
        new(Guid.Parse($"5C86B36B-6B10-4E9F-BF0E-9D66D7069{idx:D2}5"), $"{prefix}SPUP01",   $"[{prefix}] Products $30 Off - Upcoming",
            DiscountType.FixedAmount, null,  3000L, CouponScope.ItemPrice,
            now.AddDays(10).AddHours(idx),  now.AddDays(40).AddHours(idx),  false, null,   15000L, 150,  1, [1002L, 1003L], storeId, ownerId),
        new(Guid.Parse($"5C86B36B-6B10-4E9F-BF0E-9D66D7069{idx:D2}6"), $"{prefix}SPEXP01",  $"[{prefix}] Products 5% Shipping - Expired",
            DiscountType.Percentage,  5m,    null,  CouponScope.ShippingFee,
            now.AddDays(-40).AddHours(idx), now.AddDays(-10).AddHours(idx), false, 300L,   3000L,  50,   1, [1001L, 1002L], storeId, ownerId),

        // Private
        new(Guid.Parse($"5C86B36B-6B10-4E9F-BF0E-9D66D7069{idx:D2}7"), $"{prefix}PVONG01",  $"[{prefix}] Private 5% Off - Ongoing",
            DiscountType.Percentage,  5m,    null,  CouponScope.ItemPrice,
            now.AddDays(-10).AddHours(idx), now.AddDays(20).AddHours(idx),  true,  4000L,  1000L, 2000, 10, [], storeId, ownerId),
        new(Guid.Parse($"5C86B36B-6B10-4E9F-BF0E-9D66D7069{idx:D2}8"), $"{prefix}PVUP01",   $"[{prefix}] Private $25 Off - Upcoming",
            DiscountType.FixedAmount, null,  2500L, CouponScope.ItemPrice,
            now.AddDays(10).AddHours(idx),  now.AddDays(40).AddHours(idx),  true,  null,   20000L, 300,  5, [], storeId, ownerId),
        new(Guid.Parse($"5C86B36B-6B10-4E9F-BF0E-9D66D7069{idx:D2}9"), $"{prefix}PVEXP01",  $"[{prefix}] Private 8% Off - Expired",
            DiscountType.Percentage,  8m,    null,  CouponScope.ItemPrice,
            now.AddDays(-40).AddHours(idx), now.AddDays(-10).AddHours(idx), true,  2000L,  5000L,  100,  1, [], storeId, ownerId),
    ];

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
