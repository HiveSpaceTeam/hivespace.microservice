using HiveSpace.CatalogService.Domain.Aggregates.CategoryAggregate;
using HiveSpace.CatalogService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace HiveSpace.CatalogService.Infrastructure;

public static class CategorySeedData
{
    private static readonly IReadOnlyList<Category> Categories =
    [
        new(1,  "Nhà Sách Tiki",                       isActive: true, filePath: "https://salt.tikicdn.com/ts/category/ed/20/60/afa9b3b474bf7ad70f10dd6443211d5f.png"),
        new(2,  "Nhà Cửa - Đời Sống",                  isActive: true, filePath: "https://salt.tikicdn.com/ts/category/f6/22/46/7e2185d2cf1bca72d5aeac385a865b2b.png"),
        new(3,  "Điện Thoại - Máy Tính Bảng",           isActive: true, filePath: "https://salt.tikicdn.com/ts/category/54/c0/ff/fe98a4afa2d3e5142dc8096addc4e40b.png"),
        new(4,  "Đồ Chơi - Mẹ & Bé",                   isActive: true, filePath: "https://salt.tikicdn.com/ts/category/13/64/43/226301adcc7660ffcf44a61bb6df99b7.png"),
        new(5,  "Thiết Bị Số - Phụ Kiện Số",            isActive: true, filePath: "https://salt.tikicdn.com/ts/category/75/34/29/78e428fdd90408587181005f5cc3de32.png"),
        new(6,  "Điện Gia Dụng",                        isActive: true, filePath: "https://salt.tikicdn.com/ts/category/61/d4/ea/e6ea3ffc1fcde3b6224d2bb691ea16a2.png"),
        new(7,  "Làm Đẹp - Sức Khỏe",                  isActive: true, filePath: "https://salt.tikicdn.com/ts/category/73/0e/89/bf5095601d17f9971d7a08a1ffe98a42.png"),
        new(8,  "Ô Tô - Xe Máy - Xe Đạp",              isActive: true, filePath: "https://salt.tikicdn.com/ts/category/69/f5/36/c6cd9e2849854630ed74ff1678db8f19.png"),
        new(9,  "Thời Trang Nữ",                        isActive: true, filePath: "https://salt.tikicdn.com/ts/category/55/5b/80/48cbaafe144c25d5065786ecace86d38.png"),
        new(10, "Bách Hóa Online",                      isActive: true, filePath: "https://salt.tikicdn.com/ts/category/40/0f/9b/62a58fd19f540c70fce804e2a9bb5b2d.png"),
        new(11, "Thể Thao - Dã Ngoại",                  isActive: true, filePath: "https://salt.tikicdn.com/ts/category/0b/5e/3d/00941c9eb338ea62a47d5b1e042843d8.png"),
        new(12, "Thời Trang Nam",                       isActive: true, filePath: "https://salt.tikicdn.com/ts/category/00/5d/97/78713d34afa9b55826f4dc97c5e431ee.png"),
        new(13, "Cross Border - Hàng Quốc Tế",          isActive: true, filePath: "https://salt.tikicdn.com/ts/category/3c/e4/99/eeee1801c838468d94af9997ec2bbe42.png"),
        new(14, "Laptop - Máy Vi Tính - Linh Kiện",     isActive: true, filePath: "https://salt.tikicdn.com/ts/category/92/b5/c0/3ffdb7dbfafd5f8330783e1df20747f6.png"),
        new(15, "Giày - Dép Nam",                       isActive: true, filePath: "https://salt.tikicdn.com/ts/category/d6/7f/6c/5d53b60efb9448b6a1609c825c29fa40.png"),
        new(16, "Điện Tử - Điện Lạnh",                  isActive: true, filePath: "https://salt.tikicdn.com/ts/category/c8/82/d4/64c561c4ced585c74b9c292208e4995a.png"),
        new(17, "Giày - Dép Nữ",                        isActive: true, filePath: "https://salt.tikicdn.com/ts/category/cf/ed/e1/5a6b58f21fbcad0d201480c987f8defe.png"),
        new(18, "Máy Ảnh - Máy Quay Phim",              isActive: true, filePath: "https://salt.tikicdn.com/ts/category/2d/7c/45/e4976f3fa4061ab310c11d2a1b759e5b.png"),
        new(19, "Phụ Kiện Thời Trang",                  isActive: true, filePath: "https://salt.tikicdn.com/ts/category/ca/53/64/49c6189a0e1c1bf7cb91b01ff6d3fe43.png"),
        new(20, "NGON",                                  isActive: true, filePath: "https://salt.tikicdn.com/ts/category/1e/8c/08/d8b02f8a0d958c74539316e8cd437cbd.png"),
        new(21, "Đồng Hồ và Trang Sức",                 isActive: true, filePath: "https://salt.tikicdn.com/ts/category/8b/d4/a8/5924758b5c36f3b1c43b6843f52d6dd2.png"),
        new(22, "Balo và Vali",                          isActive: true, filePath: "https://salt.tikicdn.com/ts/category/3e/c0/30/1110651bd36a3e0d9b962cf135c818ee.png"),
        new(23, "Voucher - Dịch Vụ",                    isActive: true, filePath: "https://salt.tikicdn.com/ts/category/0a/c9/7b/8e466bdf6d4a5f5e14665ce56e58631d.png"),
        new(24, "Túi Thời Trang Nữ",                    isActive: true, filePath: "https://salt.tikicdn.com/ts/category/31/a7/94/6524d2ecbec216816d91b6066452e3f2.png"),
        new(25, "Túi Thời Trang Nam",                   isActive: true, filePath: "https://salt.tikicdn.com/ts/category/9b/31/af/669e6a133118e5439d6c175e27c1f963.png"),
        new(26, "Chăm Sóc Nhà Cửa",                     isActive: true, filePath: "https://salt.tikicdn.com/cache/280x280/ts/product/62/d5/9d/6be83773e4836bcbcdaf99a1750b2a28.png"),
    ];

    public static async Task SeedAsync(CatalogDbContext context, CancellationToken cancellationToken = default)
    {
        var anyExists = await context.Categories.AnyAsync(cancellationToken);
        if (anyExists)
        {
            Log.Debug("Categories already seeded. Skipping.");
            return;
        }

        await context.Categories.AddRangeAsync(Categories, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        Log.Debug("Seeded {Count} categories.", Categories.Count);
    }
}
