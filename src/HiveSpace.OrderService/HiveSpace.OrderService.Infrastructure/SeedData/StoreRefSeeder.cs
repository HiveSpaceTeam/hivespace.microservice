using HiveSpace.Infrastructure.Persistence.Seeding;
using HiveSpace.OrderService.Domain.Enumerations;
using HiveSpace.OrderService.Domain.External;
using HiveSpace.OrderService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HiveSpace.OrderService.Infrastructure.SeedData;

// Dev-bootstrap only. Ongoing sync is handled by StoreRefSyncConsumer.
internal sealed class StoreRefSeeder(OrderDbContext db, ILogger<StoreRefSeeder> logger) : ISeeder
{
    public int Order => 1;

    private static readonly (Guid StoreId, string Name, Guid OwnerId)[] Seeds =
    [
        (new Guid("b2c3d4e5-f6a7-8901-bcde-f12345678901"), "Tiki Trading",         new Guid("a1b2c3d4-e5f6-7890-abcd-ef1234567890")),
        (new Guid("e5f6a7b8-c9d0-1234-ef01-234567890123"), "GIVER BOOKS & MEDIA",   new Guid("c3d4e5f6-a7b8-9012-cdef-012345678901")),
        (new Guid("f6a7b8c9-d0e1-2345-f012-345678901234"), "Phương Đông Books",     new Guid("d4e5f6a7-b8c9-0123-def0-123456789012")),
    ];

    public async Task SeedAsync(CancellationToken ct = default)
    {
        var seedIds = Seeds.Select(s => s.StoreId).ToList();
        var existing = await db.StoreRefs
            .Where(s => seedIds.Contains(s.Id))
            .Select(s => s.Id)
            .ToHashSetAsync(ct);

        var toAdd = Seeds.Where(s => !existing.Contains(s.StoreId)).ToList();
        if (toAdd.Count == 0)
        {
            logger.LogDebug("All expected StoreRefs already exist. Skipping.");
            return;
        }

        var strategy = db.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            db.ChangeTracker.Clear();
            var currentExisting = await db.StoreRefs
                .Where(s => seedIds.Contains(s.Id))
                .Select(s => s.Id)
                .ToHashSetAsync(ct);

            var toAddNow = Seeds.Where(s => !currentExisting.Contains(s.StoreId)).ToList();
            if (toAddNow.Count == 0) return;

            await using var tx = await db.Database.BeginTransactionAsync(ct);
            foreach (var (storeId, name, ownerId) in toAddNow)
                db.StoreRefs.Add(new StoreRef(storeId, name, SellerStatus.Active, ownerId));

            await db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
        });
        logger.LogInformation("Seeded {Count} StoreRef(s).", toAdd.Count);
    }
}
