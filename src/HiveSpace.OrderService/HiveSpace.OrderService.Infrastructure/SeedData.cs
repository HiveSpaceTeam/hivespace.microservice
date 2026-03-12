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
        // Fixed IDs that match UserService seed data (seller's store)
        var storeId = new Guid("11111111-1111-1111-1111-111111111111");

        // Seed StoreRef
        if (!await context.StoreRefs.AnyAsync(s => s.Id == storeId, cancellationToken))
        {
            var store = new StoreRef(storeId, "John's Electronics Store", SellerStatus.Active);
            context.StoreRefs.Add(store);
            await context.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Seeded StoreRef: {StoreName}", store.Name);
        }
        else
        {
            logger.LogInformation("StoreRef already exists, skipping.");
        }

        // Seed ProductRefs
        const long product1Id = 1001L;
        const long product2Id = 1002L;
        const long product3Id = 1003L;

        if (!await context.ProductRefs.AnyAsync(p => p.Id == product1Id, cancellationToken))
        {
            var products = new[]
            {
                new ProductRef(product1Id, storeId, "Wireless Bluetooth Headphones", "https://example.com/images/headphones.jpg", ProductStatus.Available),
                new ProductRef(product2Id, storeId, "Mechanical Gaming Keyboard", "https://example.com/images/keyboard.jpg", ProductStatus.Available),
                new ProductRef(product3Id, storeId, "USB-C Laptop Stand", "https://example.com/images/stand.jpg", ProductStatus.Available),
            };
            context.ProductRefs.AddRange(products);
            await context.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Seeded {Count} ProductRefs", products.Length);
        }
        else
        {
            logger.LogInformation("ProductRefs already exist, skipping.");
        }

        // Seed SkuRefs
        if (!await context.SkuRefs.AnyAsync(s => s.ProductId == product1Id, cancellationToken))
        {
            var skus = new[]
            {
                // Headphones variants
                new SkuRef(10001L, product1Id, "HP-BT-BLK-001", 4999L, "USD", "https://example.com/images/headphones-black.jpg", "{\"Color\":\"Black\"}"),
                new SkuRef(10002L, product1Id, "HP-BT-WHT-001", 4999L, "USD", "https://example.com/images/headphones-white.jpg", "{\"Color\":\"White\"}"),
                new SkuRef(10003L, product1Id, "HP-BT-RED-001", 5499L, "USD", "https://example.com/images/headphones-red.jpg", "{\"Color\":\"Red\"}"),

                // Keyboard variants
                new SkuRef(10004L, product2Id, "KB-MEC-BLK-US", 8999L, "USD", "https://example.com/images/keyboard-black.jpg", "{\"Color\":\"Black\",\"Layout\":\"US\"}"),
                new SkuRef(10005L, product2Id, "KB-MEC-WHT-US", 8999L, "USD", "https://example.com/images/keyboard-white.jpg", "{\"Color\":\"White\",\"Layout\":\"US\"}"),

                // Stand variants
                new SkuRef(10006L, product3Id, "ST-USB-SLV-001", 2999L, "USD", "https://example.com/images/stand-silver.jpg", "{\"Color\":\"Silver\"}"),
                new SkuRef(10007L, product3Id, "ST-USB-BLK-001", 2999L, "USD", "https://example.com/images/stand-black.jpg", "{\"Color\":\"Black\"}"),
            };
            context.SkuRefs.AddRange(skus);
            await context.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Seeded {Count} SkuRefs", skus.Length);
        }
        else
        {
            logger.LogInformation("SkuRefs already exist, skipping.");
        }
    }
}
