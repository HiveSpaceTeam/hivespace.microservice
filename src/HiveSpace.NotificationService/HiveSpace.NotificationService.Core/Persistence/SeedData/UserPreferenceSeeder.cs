using HiveSpace.Infrastructure.Persistence.Seeding;
using HiveSpace.NotificationService.Core.DomainModels;
using HiveSpace.NotificationService.Core.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HiveSpace.NotificationService.Core.Persistence.SeedData;

internal sealed class UserPreferenceSeeder(
    NotificationDbContext         db,
    ILogger<UserPreferenceSeeder> logger) : ISeeder
{
    public int Order => 3;

    private static readonly Guid[] SellerIds =
    [
        new("a1b2c3d4-e5f6-7890-abcd-ef1234567890"),
        new("c3d4e5f6-a7b8-9012-cdef-012345678901"),
        new("d4e5f6a7-b8c9-0123-def0-123456789012"),
    ];

    private static readonly Guid[] BuyerIds =
    [
        new("11111111-1111-1111-1111-111111111111"),
        new("22222222-2222-2222-2222-222222222222"),
    ];

    private static readonly NotificationChannel[] Channels =
    [
        NotificationChannel.InApp,
        NotificationChannel.Email,
    ];

    private static readonly string[] SellerGroups =
    [
        NotificationEventGroup.SellerOrders,
        NotificationEventGroup.OrderUpdates,
        NotificationEventGroup.Inventory,
    ];

    private static readonly string[] BuyerGroups =
    [
        NotificationEventGroup.OrderUpdates,
        NotificationEventGroup.Payment,
        NotificationEventGroup.Promotions,
    ];

    public async Task SeedAsync(CancellationToken ct = default)
    {
        var allUserIds = SellerIds.Concat(BuyerIds).ToList();

        var existingChannelKeys = await db.UserChannelPreferences
            .Where(p => allUserIds.Contains(p.UserId))
            .Select(p => new { p.UserId, p.Channel })
            .ToListAsync(ct);

        var existingChannelSet = existingChannelKeys
            .Select(p => (p.UserId, p.Channel))
            .ToHashSet();

        var existingGroupKeys = await db.UserGroupPreferences
            .Where(p => allUserIds.Contains(p.UserId))
            .Select(p => new { p.UserId, p.Channel, p.EventGroup })
            .ToListAsync(ct);

        var existingGroupSet = existingGroupKeys
            .Select(p => (p.UserId, p.Channel, p.EventGroup))
            .ToHashSet();

        var channelToAdd = BuildChannelPreferences()
            .Where(p => !existingChannelSet.Contains((p.UserId, p.Channel)))
            .ToList();

        var groupToAdd = BuildGroupPreferences()
            .Where(p => !existingGroupSet.Contains((p.UserId, p.Channel, p.EventGroup)))
            .ToList();

        if (channelToAdd.Count == 0 && groupToAdd.Count == 0)
        {
            logger.LogDebug("All expected UserPreferences already exist. Skipping.");
            return;
        }

        var strategy = db.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            db.ChangeTracker.Clear();
            await using var tx = await db.Database.BeginTransactionAsync(ct);

            foreach (var pref in channelToAdd)
                db.UserChannelPreferences.Add(pref);

            foreach (var pref in groupToAdd)
                db.UserGroupPreferences.Add(pref);

            await db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
        });

        logger.LogInformation(
            "Seeded {ChannelCount} channel preference(s) and {GroupCount} group preference(s).",
            channelToAdd.Count, groupToAdd.Count);
    }

    private static IEnumerable<UserChannelPreference> BuildChannelPreferences()
    {
        foreach (var userId in SellerIds.Concat(BuyerIds))
            foreach (var channel in Channels)
                yield return UserChannelPreference.Create(userId, channel, enabled: true);
    }

    private static IEnumerable<UserGroupPreference> BuildGroupPreferences()
    {
        foreach (var userId in SellerIds)
            foreach (var channel in Channels)
                foreach (var group in SellerGroups)
                    yield return UserGroupPreference.Create(userId, channel, group, enabled: true);

        foreach (var userId in BuyerIds)
            foreach (var channel in Channels)
                foreach (var group in BuyerGroups)
                    yield return UserGroupPreference.Create(userId, channel, group, enabled: true);
    }
}
