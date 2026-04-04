using HiveSpace.CatalogService.Domain.Aggregates.External;
using HiveSpace.CatalogService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace HiveSpace.CatalogService.Infrastructure;

public static class StoreSeedData
{
    private static readonly Guid TikiStoreId       = new("b2c3d4e5-f6a7-8901-bcde-f12345678901");
    private static readonly Guid TikiSellerId      = new("a1b2c3d4-e5f6-7890-abcd-ef1234567890");

    // GIVER BOOKS & MEDIA (Tiki seller id: 2953)
    public static readonly  Guid GiverSellerId     = new("c3d4e5f6-a7b8-9012-cdef-012345678901");
    private static readonly Guid GiverStoreId      = new("e5f6a7b8-c9d0-1234-ef01-234567890123");

    // Phương Đông Books (Tiki seller id: 26874)
    public static readonly  Guid PhuongDongSellerId = new("d4e5f6a7-b8c9-0123-def0-123456789012");
    private static readonly Guid PhuongDongStoreId  = new("f6a7b8c9-d0e1-2345-f012-345678901234");

    public static async Task SeedAsync(CatalogDbContext context, CancellationToken cancellationToken = default)
    {
        var anyExists = await context.StoreRef
            .AnyAsync(s => s.OwnerId == TikiSellerId, cancellationToken);

        if (anyExists)
        {
            Log.Debug("Seed stores already exist. Skipping.");
            return;
        }

        var now = DateTimeOffset.UtcNow;

        var stores = new List<StoreRef>
        {
            new StoreRef(
                id:          TikiStoreId,
                ownerId:     TikiSellerId,
                storeName:   "Tiki Trading",
                description: "OFFICIAL_STORE • 4.7 ★ (5.5tr+ đánh giá) • 513.1k+ người theo dõi",
                logoUrl:     "https://vcdn.tikicdn.com/ts/seller/d1/3f/ae/13ce3d83ab6b6c5e77e6377ad61dc4a5.jpg",
                address:     "https://tiki.vn/cua-hang/tiki-trading",
                createdAt:   now,
                updatedAt:   now
            ),
            new StoreRef(
                id:          GiverStoreId,
                ownerId:     GiverSellerId,
                storeName:   "GIVER BOOKS & MEDIA",
                description: "OFFICIAL_STORE • 4.8 ★ (8.2k+ đánh giá) • 6.0k+ người theo dõi",
                logoUrl:     "https://vcdn.tikicdn.com/ts/seller/89/9e/7d/d19991a65a04abc9b0a410058307d255.jpg",
                address:     "https://tiki.vn/cua-hang/giver-books",
                createdAt:   now,
                updatedAt:   now
            ),
            new StoreRef(
                id:          PhuongDongStoreId,
                ownerId:     PhuongDongSellerId,
                storeName:   "Phương Đông Books",
                description: "4.8 ★ (38k+ đánh giá) • 14.5k+ người theo dõi",
                logoUrl:     "https://vcdn.tikicdn.com/ts/seller/2e/85/b7/e76104ae5f1beaf244f319e2f0d2d413.jpg",
                address:     "https://tiki.vn/cua-hang/phuong-dong-books",
                createdAt:   now,
                updatedAt:   now
            ),
        };

        await context.StoreRef.AddRangeAsync(stores, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        Log.Debug("Seeded {Count} stores.", stores.Count);
    }
}
