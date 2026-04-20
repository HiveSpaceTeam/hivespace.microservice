using HiveSpace.Infrastructure.Persistence.Seeding;
using HiveSpace.NotificationService.Core.DomainModels;
using HiveSpace.NotificationService.Core.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HiveSpace.NotificationService.Core.SeedData;

internal sealed class UserPreferenceSeeder(
    NotificationDbContext           db,
    ILogger<UserPreferenceSeeder>   logger) : ISeeder
{
    public int Order => 3;

    private static readonly Guid[] SellerIds =
    [
        new("a1b2c3d4-e5f6-7890-abcd-ef1234567890"),
        new("c3d4e5f6-a7b8-9012-cdef-012345678901"),
        new("d4e5f6a7-b8c9-0123-def0-123456789012"),
    ];

    private static readonly Guid[] CustomerIds =
    [
        new("11111111-1111-1111-1111-111111111111"), // alice
        new("22222222-2222-2222-2222-222222222222"), // bob
    ];

    // Seller event types
    private static readonly string[] SellerEventTypes =
    [
        NotificationEventType.NewOrderReceived,
        NotificationEventType.OrderConfirmed,
        NotificationEventType.OrderCancelled,
    ];

    // Customer event types
    private static readonly string[] CustomerEventTypes =
    [
        NotificationEventType.OrderConfirmed,
        NotificationEventType.OrderCancelled,
        NotificationEventType.PaymentSucceeded,
        NotificationEventType.PaymentFailed,
    ];

    private static readonly NotificationChannel[] Channels =
    [
        NotificationChannel.InApp,
        NotificationChannel.Email,
    ];

    public async Task SeedAsync(CancellationToken ct = default)
    {
        var allUserIds = SellerIds.Concat(CustomerIds).ToList();
        var existing = await db.UserPreferences
            .Where(p => allUserIds.Contains(p.UserId))
            .Select(p => new { p.UserId, p.Channel, p.EventType })
            .ToListAsync(ct);

        var existingSet = existing
            .Select(p => (p.UserId, p.Channel, p.EventType))
            .ToHashSet();

        var toAdd = BuildPreferences()
            .Where(p => !existingSet.Contains((p.UserId, p.Channel, p.EventType)))
            .ToList();

        if (toAdd.Count == 0)
        {
            logger.LogDebug("All expected UserPreferences already exist. Skipping.");
            return;
        }

        var strategy = db.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            db.ChangeTracker.Clear();

            var currentExisting = await db.UserPreferences
                .Where(p => allUserIds.Contains(p.UserId))
                .Select(p => new { p.UserId, p.Channel, p.EventType })
                .ToListAsync(ct);

            var currentSet = currentExisting
                .Select(p => (p.UserId, p.Channel, p.EventType))
                .ToHashSet();

            var toAddNow = BuildPreferences()
                .Where(p => !currentSet.Contains((p.UserId, p.Channel, p.EventType)))
                .ToList();
            if (toAddNow.Count == 0) return;

            await using var tx = await db.Database.BeginTransactionAsync(ct);
            foreach (var pref in toAddNow)
                db.UserPreferences.Add(pref);

            await db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
        });

        logger.LogInformation("Seeded {Count} UserPreference(s).", toAdd.Count);
    }

    private static IEnumerable<UserPreference> BuildPreferences()
    {
        foreach (var userId in SellerIds)
            foreach (var eventType in SellerEventTypes)
                foreach (var channel in Channels)
                    yield return UserPreference.Create(userId, channel, eventType, enabled: true);

        foreach (var userId in CustomerIds)
            foreach (var eventType in CustomerEventTypes)
                foreach (var channel in Channels)
                    yield return UserPreference.Create(userId, channel, eventType, enabled: true);
    }
}
