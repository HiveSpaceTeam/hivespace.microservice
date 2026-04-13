using HiveSpace.Infrastructure.Persistence.Seeding;
using HiveSpace.OrderService.Domain.Aggregates.Carts;
using HiveSpace.OrderService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HiveSpace.OrderService.Infrastructure.SeedData;

internal sealed class CartSeeder(OrderDbContext db, ILogger<CartSeeder> logger) : ISeeder
{
    public int Order => 4;

    private static readonly Guid AliceId     = new Guid("11111111-1111-1111-1111-111111111111");
    private static readonly Guid BobId       = new Guid("22222222-2222-2222-2222-222222222222");

    public async Task SeedAsync(CancellationToken ct = default)
    {
        var userIds = new[] { AliceId, BobId };
        var existingCarts = await db.Carts
            .Where(c => userIds.Contains(c.UserId))
            .Select(c => c.UserId)
            .ToHashSetAsync(ct);

        if (existingCarts.Count == userIds.Length)
        {
            logger.LogDebug("All expected Carts already exist. Skipping.");
            return;
        }

        var strategy = db.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            db.ChangeTracker.Clear();
            var existing = await db.Carts
                .Where(c => userIds.Contains(c.UserId))
                .Select(c => c.UserId)
                .ToHashSetAsync(ct);

            int addedCount = 0;
            await using var tx = await db.Database.BeginTransactionAsync(ct);

            if (!existing.Contains(AliceId))
            {
                var aliceCart = Cart.Create(AliceId);
                aliceCart.AddItem(1011L, 10011L, 1);
                aliceCart.AddItem(1001L, 10001L, 2);
                aliceCart.AddItem(1003L, 10003L, 1);
                db.Carts.Add(aliceCart);
                addedCount++;
            }

            if (!existing.Contains(BobId))
            {
                var bobCart = Cart.Create(BobId);
                bobCart.AddItem(1012L, 10012L, 1);
                bobCart.AddItem(1002L, 10002L, 1);
                bobCart.AddItem(1004L, 10004L, 3);
                db.Carts.Add(bobCart);
                addedCount++;
            }

            await db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
            logger.LogInformation("Seeded {Count} Carts.", addedCount);
        });
    }
}
