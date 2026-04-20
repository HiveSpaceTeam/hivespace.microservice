using HiveSpace.Domain.Shared.Enumerations;
using HiveSpace.Infrastructure.Persistence.Seeding;
using HiveSpace.OrderService.Domain.Enumerations;
using HiveSpace.OrderService.Infrastructure.Data;
using HiveSpace.OrderService.Infrastructure.Sagas;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HiveSpace.OrderService.Infrastructure.SeedData;

internal sealed class FulfillmentSagaStateSeeder(
    OrderDbContext                          db,
    ILogger<FulfillmentSagaStateSeeder>     logger) : ISeeder
{
    public int Order => 6; // After OrderSeeder

    private static readonly Guid AliceId      = new("11111111-1111-1111-1111-111111111111");
    private static readonly Guid BobId        = new("22222222-2222-2222-2222-222222222222");
    private static readonly Guid TikiStoreId  = new("b2c3d4e5-f6a7-8901-bcde-f12345678901");
    private static readonly Guid GiverStoreId = new("e5f6a7b8-c9d0-1234-ef01-234567890123");

    public async Task SeedAsync(CancellationToken ct = default)
    {
        // Query live COD orders for Alice@Tiki and Bob@Giver (fresh GUIDs each restart)
        var candidates = await db.Orders
            .Where(o => (o.UserId == AliceId && o.StoreId == TikiStoreId)
                     || (o.UserId == BobId   && o.StoreId == GiverStoreId))
            .ToListAsync(ct);

        var pendingOrders = candidates.Where(o => o.Status == OrderStatus.COD).ToList();
        if (pendingOrders.Count == 0)
        {
            logger.LogDebug("No COD PendingConfirmation orders found. Skipping FulfillmentSagaState seeding.");
            return;
        }

        var pendingIds = pendingOrders.Select(o => o.Id).ToList();

        var existing = await db.FulfillmentSagaStates
            .Where(s => pendingIds.Contains(s.CorrelationId))
            .Select(s => s.CorrelationId)
            .ToListAsync(ct);
        var existingSet = existing.ToHashSet();

        var toSeed = pendingOrders.Where(o => !existingSet.Contains(o.Id)).ToList();
        if (toSeed.Count == 0)
        {
            logger.LogDebug("All expected FulfillmentSagaStates already exist. Skipping.");
            return;
        }

        var strategy = db.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            db.ChangeTracker.Clear();

            var currentExisting = await db.FulfillmentSagaStates
                .Where(s => pendingIds.Contains(s.CorrelationId))
                .Select(s => s.CorrelationId)
                .ToListAsync(ct);
            var currentExistingSet = currentExisting.ToHashSet();

            var toAddNow = toSeed.Where(o => !currentExistingSet.Contains(o.Id)).ToList();
            if (toAddNow.Count == 0) return;

            await using var tx = await db.Database.BeginTransactionAsync(ct);

            foreach (var o in toAddNow)
            {
                db.FulfillmentSagaStates.Add(new FulfillmentSagaState
                {
                    CorrelationId                    = o.Id,
                    CurrentState                     = "WaitingForSellerConfirmation",
                    UserId                           = o.UserId,
                    StoreId                          = o.StoreId,
                    OrderCode                        = o.ShortId,
                    ReservationIds                   = [],
                    GrandTotal                       = o.TotalAmount.Amount,
                    PaymentMethod                    = PaymentMethod.COD,
                    OrderWasConfirmed                = false,
                    CreatedAt                        = DateTimeOffset.UtcNow,
                    SellerConfirmationTimeoutTokenId = null,
                    SagaStepTimeoutTokenId           = null,
                });
            }

            await db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
        });

        logger.LogInformation("Seeded {Count} FulfillmentSagaState(s).", toSeed.Count);
    }
}
