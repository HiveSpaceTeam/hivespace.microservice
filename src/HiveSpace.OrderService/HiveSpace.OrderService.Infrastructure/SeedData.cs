using HiveSpace.OrderService.Domain.Enumerations;
using HiveSpace.OrderService.Domain.External;
using HiveSpace.OrderService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

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
    }
}
