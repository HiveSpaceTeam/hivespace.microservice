using HiveSpace.CatalogService.Domain.Aggregates.External;
using HiveSpace.CatalogService.Infrastructure.Data;
using HiveSpace.Infrastructure.Persistence.Seeding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HiveSpace.CatalogService.Infrastructure.SeedData;

internal sealed class StoreSeeder(CatalogDbContext db, ILogger<StoreSeeder> logger) : ISeeder
{
    public int Order => 3;

    public async Task SeedAsync(CancellationToken ct = default)
    {
        var anyExists = await db.StoreRef.AnyAsync(
            s => s.OwnerId == SeedConstants.TikiSellerId
              || s.OwnerId == SeedConstants.GiverSellerId
              || s.OwnerId == SeedConstants.PhuongDongSellerId,
            ct);

        if (anyExists)
        {
            logger.LogDebug("Seed stores already exist. Skipping.");
            return;
        }

        var now = DateTimeOffset.UtcNow;
        var stores = new List<StoreRef>
        {
            new(
                id:          new Guid("b2c3d4e5-f6a7-8901-bcde-f12345678901"),
                ownerId:     SeedConstants.TikiSellerId,
                storeName:   "Tiki Trading",
                description: "OFFICIAL_STORE • 4.7 ★ (5.5tr+ đánh giá) • 513.1k+ người theo dõi",
                logoUrl:     "https://vcdn.tikicdn.com/ts/seller/d1/3f/ae/13ce3d83ab6b6c5e77e6377ad61dc4a5.jpg",
                address:     "https://tiki.vn/cua-hang/tiki-trading",
                createdAt:   now,
                updatedAt:   now
            ),
            new(
                id:          new Guid("e5f6a7b8-c9d0-1234-ef01-234567890123"),
                ownerId:     SeedConstants.GiverSellerId,
                storeName:   "GIVER BOOKS & MEDIA",
                description: "OFFICIAL_STORE • 4.8 ★ (8.2k+ đánh giá) • 6.0k+ người theo dõi",
                logoUrl:     "https://vcdn.tikicdn.com/ts/seller/89/9e/7d/d19991a65a04abc9b0a410058307d255.jpg",
                address:     "https://tiki.vn/cua-hang/giver-books",
                createdAt:   now,
                updatedAt:   now
            ),
            new(
                id:          new Guid("f6a7b8c9-d0e1-2345-f012-345678901234"),
                ownerId:     SeedConstants.PhuongDongSellerId,
                storeName:   "Phương Đông Books",
                description: "4.8 ★ (38k+ đánh giá) • 14.5k+ người theo dõi",
                logoUrl:     "https://vcdn.tikicdn.com/ts/seller/2e/85/b7/e76104ae5f1beaf244f319e2f0d2d413.jpg",
                address:     "https://tiki.vn/cua-hang/phuong-dong-books",
                createdAt:   now,
                updatedAt:   now
            ),
        };

        var strategy = db.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await using var tx = await db.Database.BeginTransactionAsync(ct);
            await db.StoreRef.AddRangeAsync(stores, ct);
            await db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
        });
        logger.LogInformation("Seeded {Count} stores.", stores.Count);
    }
}
