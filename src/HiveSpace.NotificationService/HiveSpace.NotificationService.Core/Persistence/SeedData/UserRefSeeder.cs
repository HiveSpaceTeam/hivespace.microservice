using HiveSpace.Infrastructure.Persistence.Seeding;
using HiveSpace.NotificationService.Core.DomainModels.External;
using HiveSpace.NotificationService.Core.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HiveSpace.NotificationService.Core.Persistence.SeedData;

internal sealed class UserRefSeeder(
    NotificationDbContext       db,
    ILogger<UserRefSeeder>      logger) : ISeeder
{
    public int Order => 2;

    private static readonly (Guid UserId, string Email, string FullName, string Locale, string? UserName, string? AvatarUrl, Guid? StoreId, string? StoreName, string? StoreLogoUrl)[] Seeds =
    [
        (new Guid("a1b2c3d4-e5f6-7890-abcd-ef1234567890"), "dev5k@gmail.com",       "Tiki Trading",        "vi", "tiki",       null,
            new Guid("b2c3d4e5-f6a7-8901-bcde-f12345678901"), "Tiki Trading",
            "https://vcdn.tikicdn.com/ts/seller/d1/3f/ae/13ce3d83ab6b6c5e77e6377ad61dc4a5.jpg"),
        (new Guid("c3d4e5f6-a7b8-9012-cdef-012345678901"), "giver@gmail.com",      "GIVER BOOKS & MEDIA", "vi", "giver",      null,
            new Guid("e5f6a7b8-c9d0-1234-ef01-234567890123"), "GIVER BOOKS & MEDIA",
            "https://vcdn.tikicdn.com/ts/seller/89/9e/7d/d19991a65a04abc9b0a410058307d255.jpg"),
        (new Guid("d4e5f6a7-b8c9-0123-def0-123456789012"), "phuongdong@gmail.com", "Phương Đông Books",   "vi", "phuongdong", null,
            new Guid("f6a7b8c9-d0e1-2345-f012-345678901234"), "Phương Đông Books",
            "https://vcdn.tikicdn.com/ts/seller/2e/85/b7/e76104ae5f1beaf244f319e2f0d2d413.jpg"),
        (new Guid("11111111-1111-1111-1111-111111111111"), "aliceSmith@gmail.com", "Alice Smith",         "vi", "alice",      "https://cdn.discordapp.com/avatars/474579515188707339/24acc2c6b645216504447360a58c0683.webp",
            null, null, null),
        (new Guid("22222222-2222-2222-2222-222222222222"), "bobSmith@gmail.com",   "Bob Smith",           "vi", "bob",        "https://cdn.discordapp.com/avatars/743061397037908059/edaa33d7f618405017a26cc7ca42379b.webp",
            null, null, null),
    ];

    public async Task SeedAsync(CancellationToken ct = default)
    {
        var seedIds = Seeds.Select(s => s.UserId).ToList();
        var existing = await db.UserRefs
            .Where(u => seedIds.Contains(u.Id))
            .Select(u => u.Id)
            .ToListAsync(ct);
        var existingSet = existing.ToHashSet();

        var toAdd = Seeds.Where(s => !existingSet.Contains(s.UserId)).ToList();
        if (toAdd.Count == 0)
        {
            logger.LogDebug("All expected UserRefs already exist. Skipping.");
            return;
        }

        var strategy = db.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            db.ChangeTracker.Clear();

            var currentExisting = await db.UserRefs
                .Where(u => seedIds.Contains(u.Id))
                .Select(u => u.Id)
                .ToListAsync(ct);
            var currentExistingSet = currentExisting.ToHashSet();

            var toAddNow = Seeds.Where(s => !currentExistingSet.Contains(s.UserId)).ToList();
            if (toAddNow.Count == 0) return;

            await using var tx = await db.Database.BeginTransactionAsync(ct);
            foreach (var (userId, email, fullName, locale, userName, avatarUrl, storeId, storeName, storeLogoUrl) in toAddNow)
                db.UserRefs.Add(UserRef.Create(userId, email, fullName,
                    locale: locale, userName: userName, avatarUrl: avatarUrl,
                    storeId: storeId, storeName: storeName, storeLogoUrl: storeLogoUrl));

            await db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
        });

        logger.LogInformation("Seeded {Count} UserRef(s).", toAdd.Count);
    }
}
