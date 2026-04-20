using HiveSpace.Domain.Shared.ValueObjects;
using HiveSpace.Infrastructure.Persistence.Seeding;
using HiveSpace.PaymentService.Domain.Aggregates.Wallets;
using HiveSpace.PaymentService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HiveSpace.PaymentService.Infrastructure.SeedData;

internal sealed class WalletSeeder(PaymentDbContext db, ILogger<WalletSeeder> logger) : ISeeder
{
    public int Order => 1;

    // Matches seeded user IDs in UserService / OrderService
    private static readonly Guid AliceId = new("11111111-1111-1111-1111-111111111111");
    private static readonly Guid BobId   = new("22222222-2222-2222-2222-222222222222");

    public async Task SeedAsync(CancellationToken ct = default)
    {
        var seededIds = new[] { AliceId, BobId };
        var existingIds = await db.Wallets
            .Where(w => seededIds.Contains(w.UserId))
            .Select(w => w.UserId)
            .ToListAsync(ct);
        var existingIdSet = existingIds.ToHashSet();

        if (existingIdSet.Contains(AliceId) && existingIdSet.Contains(BobId))
        {
            logger.LogDebug("Wallets already seeded. Skipping.");
            return;
        }

        var strategy = db.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            db.ChangeTracker.Clear();
            var existing = await db.Wallets
                .Where(w => seededIds.Contains(w.UserId))
                .Select(w => w.UserId)
                .ToListAsync(ct);
            var existingSet = existing.ToHashSet();

            await using var tx = await db.Database.BeginTransactionAsync(ct);

            if (!existingSet.Contains(AliceId))
            {
                var aliceWallet = Wallet.CreateForUser(AliceId);
                aliceWallet.Credit(Money.Create(5_000_000, "VND"), "SEED-TOPUP", "Initial wallet top-up");
                db.Wallets.Add(aliceWallet);
            }

            if (!existingSet.Contains(BobId))
            {
                var bobWallet = Wallet.CreateForUser(BobId);
                bobWallet.Credit(Money.Create(3_000_000, "VND"), "SEED-TOPUP", "Initial wallet top-up");
                db.Wallets.Add(bobWallet);
            }

            await db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
        });

        logger.LogInformation("Seeded Wallet(s) via WalletSeeder.");
    }
}
