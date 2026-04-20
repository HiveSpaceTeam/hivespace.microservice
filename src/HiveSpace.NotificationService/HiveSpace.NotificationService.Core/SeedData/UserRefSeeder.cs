using HiveSpace.Infrastructure.Persistence.Seeding;
using HiveSpace.NotificationService.Core.DomainModels.External;
using HiveSpace.NotificationService.Core.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HiveSpace.NotificationService.Core.SeedData;

internal sealed class UserRefSeeder(
    NotificationDbContext       db,
    ILogger<UserRefSeeder>      logger) : ISeeder
{
    public int Order => 2;

    // GUIDs match the seeded accounts in UserService SeedData (sellers + Alice/Bob customers).
    private static readonly (Guid UserId, string Email, string FullName, string Locale, string? UserName, string? AvatarUrl, Guid? StoreId, string? StoreName, string? StoreLogoUrl)[] Seeds =
    [
        (new Guid("a1b2c3d4-e5f6-7890-abcd-ef1234567890"), "tiki@gmail.com",       "Tiki Trading",        "vi", "tiki",       null,
            new Guid("b2c3d4e5-f6a7-8901-bcde-f12345678901"), "Tiki Trading",
            "https://vcdn.tikicdn.com/ts/seller/d1/3f/ae/13ce3d83ab6b6c5e77e6377ad61dc4a5.jpg"),
        (new Guid("c3d4e5f6-a7b8-9012-cdef-012345678901"), "giver@gmail.com",      "GIVER BOOKS & MEDIA", "vi", "giver",      null,
            new Guid("e5f6a7b8-c9d0-1234-ef01-234567890123"), "GIVER BOOKS & MEDIA",
            "https://vcdn.tikicdn.com/ts/seller/89/9e/7d/d19991a65a04abc9b0a410058307d255.jpg"),
        (new Guid("d4e5f6a7-b8c9-0123-def0-123456789012"), "phuongdong@gmail.com", "Phương Đông Books",   "vi", "phuongdong", null,
            new Guid("f6a7b8c9-d0e1-2345-f012-345678901234"), "Phương Đông Books",
            "https://vcdn.tikicdn.com/ts/seller/2e/85/b7/e76104ae5f1beaf244f319e2f0d2d413.jpg"),
        (new Guid("11111111-1111-1111-1111-111111111111"), "aliceSmith@gmail.com", "Alice Smith",         "vi", "alice",      "https://scontent.fhan7-1.fna.fbcdn.net/v/t39.30808-1/529742079_777341701485971_4020105096641008833_n.jpg?stp=dst-jpg_s200x200_tt6&_nc_cat=106&ccb=1-7&_nc_sid=e99d92&_nc_eui2=AeHOZtTR0YEI1wTAoLhcXxYrZ_lDYDRtnbpn-UNgNG2dujEOqjbJP2jUyZLseGPjOw7m1we5cC62ulwjACTvYMP5&_nc_ohc=u4NsqmH7Ci0Q7kNvwF1-Wi5&_nc_oc=AdrxQ0nCifqlTK4bIxsGMxJIMtJMRNU-mIbx0fPKus5W9-JxM4vEoYZ6jbqIY4FIAIRn0jRamFEn9EGMuNuWWXTt&_nc_zt=24&_nc_ht=scontent.fhan7-1.fna&_nc_gid=_2qgRLt6g8Es9mmKIN6Vlw&_nc_ss=7a3a8&oh=00_Af02at1L9pVGXBbLQ1TXaFSwosarmT2Yv3kwgwda-cFdXg&oe=69E92E54",
            null, null, null),
        (new Guid("22222222-2222-2222-2222-222222222222"), "bobSmith@gmail.com",   "Bob Smith",           "vi", "bob",        "https://scontent.fhan7-1.fna.fbcdn.net/v/t39.30808-1/619108846_2920789344795889_1339672122962744350_n.jpg?stp=dst-jpg_s200x200_tt6&_nc_cat=108&ccb=1-7&_nc_sid=e99d92&_nc_eui2=AeGRyZ28lkuXPO966VWTa7-WXJ5jeRHvuA1cnmN5Ee-4DdkXY30zOg5G3Xv5E85oBsdjDBCC81n8TavngkrBmWN7&_nc_ohc=DKnUNDzXPSsQ7kNvwFeEkU5&_nc_oc=AdqslpzU_dqjl89K9X3s70qGjddv8Pi7HzRhAwpQifj7tTJIsSxyG_Ro__Yt2IR87m3-3tCirslSOKeotp7gGJMT&_nc_zt=24&_nc_ht=scontent.fhan7-1.fna&_nc_gid=NgW59NCpuV6NO_P9hLKcew&_nc_ss=7a3a8&oh=00_Af1SoYAMjpm3uOR1QS87h6G3obpvpSgeq6gMaaE3MCDTYQ&oe=69E945C0",
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
