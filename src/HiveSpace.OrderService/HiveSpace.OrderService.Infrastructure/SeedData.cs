using HiveSpace.OrderService.Domain.Enumerations;
using HiveSpace.OrderService.Domain.External;
using HiveSpace.OrderService.Domain.Aggregates.Coupons;
using HiveSpace.OrderService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using HiveSpace.Domain.Shared.Enumerations;
using HiveSpace.Domain.Shared.ValueObjects;

namespace HiveSpace.OrderService.Infrastructure;

public class SeedData
{
    public static async Task EnsureSeedDataAsync(WebApplication app, CancellationToken cancellationToken = default)
    {
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<SeedData>>();

        Console.WriteLine("Checking for pending migrations...");
        var pending = (await context.Database.GetPendingMigrationsAsync(cancellationToken)).ToList();
        if (pending.Count > 0)
        {
            logger.LogInformation("Found {Count} pending migrations: {Migrations}", pending.Count, string.Join(", ", pending));
            Console.WriteLine($"Found {pending.Count} pending migrations. Applying...");

            await context.Database.MigrateAsync(cancellationToken);

            logger.LogInformation("Migrations applied successfully");
            Console.WriteLine("Migrations applied successfully.");
        }
        else
        {
            logger.LogInformation("No pending migrations found. Database is up to date.");
            Console.WriteLine("No pending migrations found. Database is up to date.");
        }

        await SeedReferenceDataAsync(context, logger, cancellationToken);
    }

    private static async Task SeedReferenceDataAsync(OrderDbContext context, ILogger<SeedData> logger, CancellationToken cancellationToken)
    {
        const string storeName = "John's Electronics Store";

        // Resolve existing StoreRef identity by business key (name) and only create if missing.
        var existingStore = await context.StoreRefs
            .SingleOrDefaultAsync(s => s.Name == storeName, cancellationToken);

        Guid storeId;
        if (existingStore is null)
        {
            var store = new StoreRef(Guid.NewGuid(), storeName, SellerStatus.Active);
            context.StoreRefs.Add(store);
            await context.SaveChangesAsync(cancellationToken);
            storeId = store.Id;
            logger.LogInformation("Seeded StoreRef: {StoreName} ({StoreId})", store.Name, store.Id);
        }
        else
        {
            storeId = existingStore.Id;
            logger.LogInformation("Reusing existing StoreRef: {StoreName} ({StoreId})", existingStore.Name, existingStore.Id);
        }

        // Seed ProductRefs
        const long product1Id = 1001L;
        const long product2Id = 1002L;
        const long product3Id = 1003L;

        var expectedProductIds = new[] { product1Id, product2Id, product3Id };
        var existingProductIds = await context.ProductRefs
            .Where(p => expectedProductIds.Contains(p.Id))
            .Select(p => p.Id)
            .ToListAsync(cancellationToken);

        var missingProductIds = expectedProductIds.Except(existingProductIds).ToList();
        if (missingProductIds.Count > 0)
        {
            var productsById = new Dictionary<long, ProductRef>
            {
                [product1Id] = new ProductRef(product1Id, storeId, "Wireless Bluetooth Headphones", "https://example.com/images/headphones.jpg", ProductStatus.Available),
                [product2Id] = new ProductRef(product2Id, storeId, "Mechanical Gaming Keyboard", "https://example.com/images/keyboard.jpg", ProductStatus.Available),
                [product3Id] = new ProductRef(product3Id, storeId, "USB-C Laptop Stand", "https://example.com/images/stand.jpg", ProductStatus.Available),
            };

            var missingProducts = missingProductIds.Select(id => productsById[id]).ToList();
            context.ProductRefs.AddRange(missingProducts);
            await context.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Seeded {Count} missing ProductRefs", missingProducts.Count);
        }
        else
        {
            logger.LogInformation("All expected ProductRefs already exist, skipping.");
        }

        // Seed SkuRefs
        var expectedSkuIds = new[]
        {
            10001L, 10002L, 10003L, 10004L, 10005L, 10006L, 10007L
        };

        var existingSkuIds = await context.SkuRefs
            .Where(s => expectedSkuIds.Contains(s.Id))
            .Select(s => s.Id)
            .ToListAsync(cancellationToken);

        var missingSkuIds = expectedSkuIds.Except(existingSkuIds).ToList();
        if (missingSkuIds.Count > 0)
        {
            var skusById = new Dictionary<long, SkuRef>
            {
                [10001L] = new SkuRef(10001L, product1Id, "HP-BT-BLK-001", 4999L, "USD", "https://example.com/images/headphones-black.jpg", "{\"Color\":\"Black\"}"),
                [10002L] = new SkuRef(10002L, product1Id, "HP-BT-WHT-001", 4999L, "USD", "https://example.com/images/headphones-white.jpg", "{\"Color\":\"White\"}"),
                [10003L] = new SkuRef(10003L, product1Id, "HP-BT-RED-001", 5499L, "USD", "https://example.com/images/headphones-red.jpg", "{\"Color\":\"Red\"}"),
                [10004L] = new SkuRef(10004L, product2Id, "KB-MEC-BLK-US", 8999L, "USD", "https://example.com/images/keyboard-black.jpg", "{\"Color\":\"Black\",\"Layout\":\"US\"}"),
                [10005L] = new SkuRef(10005L, product2Id, "KB-MEC-WHT-US", 8999L, "USD", "https://example.com/images/keyboard-white.jpg", "{\"Color\":\"White\",\"Layout\":\"US\"}"),
                [10006L] = new SkuRef(10006L, product3Id, "ST-USB-SLV-001", 2999L, "USD", "https://example.com/images/stand-silver.jpg", "{\"Color\":\"Silver\"}"),
                [10007L] = new SkuRef(10007L, product3Id, "ST-USB-BLK-001", 2999L, "USD", "https://example.com/images/stand-black.jpg", "{\"Color\":\"Black\"}"),
            };

            var missingSkus = missingSkuIds.Select(id => skusById[id]).ToList();
            context.SkuRefs.AddRange(missingSkus);
            await context.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Seeded {Count} missing SkuRefs", missingSkus.Count);
        }
        else
        {
            logger.LogInformation("All expected SkuRefs already exist, skipping.");
        }

        await SeedCouponsAsync(context, logger, storeId, cancellationToken);
    }

    private static async Task SeedCouponsAsync(
        OrderDbContext context,
        ILogger<SeedData> logger,
        Guid storeId,
        CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var seedStoreOwnerId = Guid.Parse("11111111-1111-1111-1111-111111111111");

        var templates = new[]
        {
            // Entire shop: Ongoing / Upcoming / Expired
            new
            {
                Code = "ESONG01",
                Name = "Entire Shop 10% Off - Ongoing",
                DiscountType = DiscountType.Percentage,
                Percentage = (decimal?)10m,
                FixedAmountUsd = (decimal?)null,
                Scope = CouponScope.ItemPrice,
                StartDateTime = now.AddDays(-10),
                EndDateTime = now.AddDays(20),
                IsHidden = false,
                MaxDiscountUsd = (decimal?)50m,
                MinOrderUsd = 20m,
                MaxUsageCount = 1000,
                MaxUsagePerUser = 3,
                ProductIds = Array.Empty<long>()
            },
            new
            {
                Code = "ESUP01",
                Name = "Entire Shop $20 Off - Upcoming",
                DiscountType = DiscountType.FixedAmount,
                Percentage = (decimal?)null,
                FixedAmountUsd = (decimal?)20m,
                Scope = CouponScope.ItemPrice,
                StartDateTime = now.AddDays(10),
                EndDateTime = now.AddDays(40),
                IsHidden = false,
                MaxDiscountUsd = (decimal?)null,
                MinOrderUsd = 100m,
                MaxUsageCount = 500,
                MaxUsagePerUser = 2,
                ProductIds = Array.Empty<long>()
            },
            new
            {
                Code = "ESEXP01",
                Name = "Entire Shop $5 Shipping - Expired",
                DiscountType = DiscountType.FixedAmount,
                Percentage = (decimal?)null,
                FixedAmountUsd = (decimal?)5m,
                Scope = CouponScope.ShippingFee,
                StartDateTime = now.AddDays(-40),
                EndDateTime = now.AddDays(-10),
                IsHidden = false,
                MaxDiscountUsd = (decimal?)null,
                MinOrderUsd = 50m,
                MaxUsageCount = 100,
                MaxUsagePerUser = 1,
                ProductIds = Array.Empty<long>()
            },

            // Specific products: Ongoing / Upcoming / Expired
            new
            {
                Code = "SPONG01",
                Name = "Products 15% Off - Ongoing",
                DiscountType = DiscountType.Percentage,
                Percentage = (decimal?)15m,
                FixedAmountUsd = (decimal?)null,
                Scope = CouponScope.ItemPrice,
                StartDateTime = now.AddDays(-10),
                EndDateTime = now.AddDays(20),
                IsHidden = false,
                MaxDiscountUsd = (decimal?)25m,
                MinOrderUsd = 20m,
                MaxUsageCount = 200,
                MaxUsagePerUser = 2,
                ProductIds = new[] { 1001L }
            },
            new
            {
                Code = "SPUP01",
                Name = "Products $30 Off - Upcoming",
                DiscountType = DiscountType.FixedAmount,
                Percentage = (decimal?)null,
                FixedAmountUsd = (decimal?)30m,
                Scope = CouponScope.ItemPrice,
                StartDateTime = now.AddDays(10),
                EndDateTime = now.AddDays(40),
                IsHidden = false,
                MaxDiscountUsd = (decimal?)null,
                MinOrderUsd = 150m,
                MaxUsageCount = 150,
                MaxUsagePerUser = 1,
                ProductIds = new[] { 1002L, 1003L }
            },
            new
            {
                Code = "SPEXP01",
                Name = "Products 5% Shipping - Expired",
                DiscountType = DiscountType.Percentage,
                Percentage = (decimal?)5m,
                FixedAmountUsd = (decimal?)null,
                Scope = CouponScope.ShippingFee,
                StartDateTime = now.AddDays(-40),
                EndDateTime = now.AddDays(-10),
                IsHidden = false,
                MaxDiscountUsd = (decimal?)3m,
                MinOrderUsd = 30m,
                MaxUsageCount = 50,
                MaxUsagePerUser = 1,
                ProductIds = new[] { 1001L, 1002L }
            },

            // Private: Ongoing / Upcoming / Expired
            new
            {
                Code = "PVONG01",
                Name = "Private 5% Off - Ongoing",
                DiscountType = DiscountType.Percentage,
                Percentage = (decimal?)5m,
                FixedAmountUsd = (decimal?)null,
                Scope = CouponScope.ItemPrice,
                StartDateTime = now.AddDays(-10),
                EndDateTime = now.AddDays(20),
                IsHidden = true,
                MaxDiscountUsd = (decimal?)40m,
                MinOrderUsd = 10m,
                MaxUsageCount = 2000,
                MaxUsagePerUser = 10,
                ProductIds = Array.Empty<long>()
            },
            new
            {
                Code = "PVUP01",
                Name = "Private $25 Off - Upcoming",
                DiscountType = DiscountType.FixedAmount,
                Percentage = (decimal?)null,
                FixedAmountUsd = (decimal?)25m,
                Scope = CouponScope.ItemPrice,
                StartDateTime = now.AddDays(10),
                EndDateTime = now.AddDays(40),
                IsHidden = true,
                MaxDiscountUsd = (decimal?)null,
                MinOrderUsd = 200m,
                MaxUsageCount = 300,
                MaxUsagePerUser = 5,
                ProductIds = Array.Empty<long>()
            },
            new
            {
                Code = "PVEXP01",
                Name = "Private 8% Off - Expired",
                DiscountType = DiscountType.Percentage,
                Percentage = (decimal?)8m,
                FixedAmountUsd = (decimal?)null,
                Scope = CouponScope.ItemPrice,
                StartDateTime = now.AddDays(-40),
                EndDateTime = now.AddDays(-10),
                IsHidden = true,
                MaxDiscountUsd = (decimal?)20m,
                MinOrderUsd = 50m,
                MaxUsageCount = 100,
                MaxUsagePerUser = 1,
                ProductIds = Array.Empty<long>()
            }
        };

        var expectedCodes = templates.Select(t => t.Code).ToList();
        var existingCodes = await context.Coupons
            .Where(c => expectedCodes.Contains(c.Code))
            .Select(c => c.Code)
            .ToListAsync(cancellationToken);

        var existingCodeSet = new HashSet<string>(existingCodes, StringComparer.OrdinalIgnoreCase);
        var missingTemplates = templates.Where(t => !existingCodeSet.Contains(t.Code)).ToList();

        if (missingTemplates.Count == 0)
        {
            logger.LogInformation("All expected Coupons already exist, skipping.");
            return;
        }

        var missingCoupons = new List<Coupon>();
        foreach (var template in missingTemplates)
        {
            var coupon = Coupon.CreateByStore(
                storeId,
                seedStoreOwnerId,
                template.Code,
                template.Name,
                template.DiscountType,
                template.Percentage,
                template.FixedAmountUsd.HasValue ? Money.FromUSD(template.FixedAmountUsd.Value) : null,
                template.Scope,
                template.StartDateTime,
                template.EndDateTime,
                earlySaveDateTime: null,
                isHidden: template.IsHidden,
                maxDiscountAmount: template.MaxDiscountUsd.HasValue ? Money.FromUSD(template.MaxDiscountUsd.Value) : null,
                minOrderAmount: Money.FromUSD(template.MinOrderUsd));

            coupon.SetMaxUsageCount(template.MaxUsageCount);
            coupon.SetMaxUsagePerUser(template.MaxUsagePerUser);

            if (template.ProductIds.Length > 0)
            {
                coupon.LimitToProducts(template.ProductIds);
            }

            missingCoupons.Add(coupon);
        }

        context.Coupons.AddRange(missingCoupons);
        await context.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Seeded {Count} missing Coupons", missingCoupons.Count);
    }
}
