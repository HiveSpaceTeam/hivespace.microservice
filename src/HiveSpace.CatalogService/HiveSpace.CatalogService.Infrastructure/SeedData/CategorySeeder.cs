using HiveSpace.CatalogService.Domain.Aggregates.CategoryAggregate;
using HiveSpace.CatalogService.Infrastructure.Data;
using HiveSpace.Infrastructure.Persistence.Seeding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HiveSpace.CatalogService.Infrastructure.SeedData;

internal sealed class CategorySeeder(CatalogDbContext db, ILogger<CategorySeeder> logger) : ISeeder
{
    public int Order => 1;

    private static readonly IReadOnlyList<(string Name, string FilePath)> CategorySeeds =
    [
        ("Nhà Sách Tiki",                       "https://salt.tikicdn.com/ts/category/ed/20/60/afa9b3b474bf7ad70f10dd6443211d5f.png"),
        ("Nhà Cửa - Đời Sống",                  "https://salt.tikicdn.com/ts/category/f6/22/46/7e2185d2cf1bca72d5aeac385a865b2b.png"),
        ("Điện Thoại - Máy Tính Bảng",           "https://salt.tikicdn.com/ts/category/54/c0/ff/fe98a4afa2d3e5142dc8096addc4e40b.png"),
        ("Đồ Chơi - Mẹ & Bé",                   "https://salt.tikicdn.com/ts/category/13/64/43/226301adcc7660ffcf44a61bb6df99b7.png"),
        ("Thiết Bị Số - Phụ Kiện Số",            "https://salt.tikicdn.com/ts/category/75/34/29/78e428fdd90408587181005f5cc3de32.png"),
        ("Điện Gia Dụng",                        "https://salt.tikicdn.com/ts/category/61/d4/ea/e6ea3ffc1fcde3b6224d2bb691ea16a2.png"),
        ("Làm Đẹp - Sức Khỏe",                  "https://salt.tikicdn.com/ts/category/73/0e/89/bf5095601d17f9971d7a08a1ffe98a42.png"),
        ("Ô Tô - Xe Máy - Xe Đạp",              "https://salt.tikicdn.com/ts/category/69/f5/36/c6cd9e2849854630ed74ff1678db8f19.png"),
        ("Thời Trang Nữ",                        "https://salt.tikicdn.com/ts/category/55/5b/80/48cbaafe144c25d5065786ecace86d38.png"),
        ("Bách Hóa Online",                      "https://salt.tikicdn.com/ts/category/40/0f/9b/62a58fd19f540c70fce804e2a9bb5b2d.png"),
        ("Thể Thao - Dã Ngoại",                  "https://salt.tikicdn.com/ts/category/0b/5e/3d/00941c9eb338ea62a47d5b1e042843d8.png"),
        ("Thời Trang Nam",                       "https://salt.tikicdn.com/ts/category/00/5d/97/78713d34afa9b55826f4dc97c5e431ee.png"),
        ("Cross Border - Hàng Quốc Tế",          "https://salt.tikicdn.com/ts/category/3c/e4/99/eeee1801c838468d94af9997ec2bbe42.png"),
        ("Laptop - Máy Vi Tính - Linh Kiện",     "https://salt.tikicdn.com/ts/category/92/b5/c0/3ffdb7dbfafd5f8330783e1df20747f6.png"),
        ("Giày - Dép Nam",                       "https://salt.tikicdn.com/ts/category/d6/7f/6c/5d53b60efb9448b6a1609c825c29fa40.png"),
        ("Điện Tử - Điện Lạnh",                  "https://salt.tikicdn.com/ts/category/c8/82/d4/64c561c4ced585c74b9c292208e4995a.png"),
        ("Giày - Dép Nữ",                        "https://salt.tikicdn.com/ts/category/cf/ed/e1/5a6b58f21fbcad0d201480c987f8defe.png"),
        ("Máy Ảnh - Máy Quay Phim",              "https://salt.tikicdn.com/ts/category/2d/7c/45/e4976f3fa4061ab310c11d2a1b759e5b.png"),
        ("Phụ Kiện Thời Trang",                  "https://salt.tikicdn.com/ts/category/ca/53/64/49c6189a0e1c1bf7cb91b01ff6d3fe43.png"),
        ("NGON",                                  "https://salt.tikicdn.com/ts/category/1e/8c/08/d8b02f8a0d958c74539316e8cd437cbd.png"),
        ("Đồng Hồ và Trang Sức",                 "https://salt.tikicdn.com/ts/category/8b/d4/a8/5924758b5c36f3b1c43b6843f52d6dd2.png"),
        ("Balo và Vali",                          "https://salt.tikicdn.com/ts/category/3e/c0/30/1110651bd36a3e0d9b962cf135c818ee.png"),
        ("Voucher - Dịch Vụ",                    "https://salt.tikicdn.com/ts/category/0a/c9/7b/8e466bdf6d4a5f5e14665ce56e58631d.png"),
        ("Túi Thời Trang Nữ",                    "https://salt.tikicdn.com/ts/category/31/a7/94/6524d2ecbec216816d91b6066452e3f2.png"),
        ("Túi Thời Trang Nam",                   "https://salt.tikicdn.com/ts/category/9b/31/af/669e6a133118e5439d6c175e27c1f963.png"),
        ("Chăm Sóc Nhà Cửa",                     "https://salt.tikicdn.com/cache/280x280/ts/product/62/d5/9d/6be83773e4836bcbcdaf99a1750b2a28.png"),
    ];

    public async Task SeedAsync(CancellationToken ct = default)
    {
        var anyExists = await db.Categories.AnyAsync(ct);
        if (anyExists)
        {
            logger.LogDebug("Categories already seeded. Skipping.");
            return;
        }

        var categories = CategorySeeds
            .Select((category, index) => new Category(
                id: index + 1,
                name: category.Name,
                isActive: true,
                filePath: category.FilePath))
            .ToList();

        var strategy = db.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await using var tx = await db.Database.BeginTransactionAsync(ct);
            await db.Categories.AddRangeAsync(categories, ct);
            await db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
        });
        logger.LogInformation("Seeded {Count} categories.", categories.Count);
    }
}
