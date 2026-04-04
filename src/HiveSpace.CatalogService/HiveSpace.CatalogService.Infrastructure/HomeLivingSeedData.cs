using HiveSpace.CatalogService.Domain.Aggregates.ProductAggregate;
using HiveSpace.CatalogService.Domain.Common;
using HiveSpace.CatalogService.Domain.Enums;
using HiveSpace.CatalogService.Infrastructure.Data;
using HiveSpace.Domain.Shared.Enumerations;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace HiveSpace.CatalogService.Infrastructure;

public static class HomeLivingSeedData
{
    private static readonly Guid SeedSellerId = new("a1b2c3d4-e5f6-7890-abcd-ef1234567890");

    private const int HomeLivingCategoryId = 2;

    public static async Task SeedAsync(CatalogDbContext context, CancellationToken cancellationToken = default)
    {
        var anyExists = await context.Products
            .Where(p => p.Categories.Any(c => c.CategoryId == HomeLivingCategoryId))
            .AnyAsync(cancellationToken);
        if (anyExists)
        {
            Log.Debug("Nhà Cửa - Đời Sống products already seeded. Skipping.");
            return;
        }

        var products = BuildProducts();
        await context.Products.AddRangeAsync(products, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        Log.Debug("Seeded {Count} products for Nhà Cửa - Đời Sống.", products.Count);
    }

    private static List<Product> BuildProducts()
    {
        var now = DateTimeOffset.UtcNow;

        return
        [
            Build(
                name: "Cốc Giữ Nhiệt Inox 304 Elmich EL8345 Dung Tích 480ML - Hàng Chính Hãng",
                slug: "coc-giu-nhiet-inox-304-elmich-el8345-dung-tich-480ml-hang-chinh-hang",
                description: "Cốc giữ nhiệt Elmich EL8345 dung tích 480ml làm từ inox 304 cao cấp, an toàn cho sức khỏe. Thiết kế sang trọng, tiện lợi mang theo mọi nơi. Nắp chống tràn thông minh, giữ nhiệt hiệu quả cho cả đồ uống nóng và lạnh. Có nhiều màu sắc để lựa chọn.",
                shortDescription: "Cốc giữ nhiệt inox 304 Elmich EL8345 480ml, nắp chống tràn, nhiều màu.",
                skuNo: "9916475726151",
                price: 113240m,
                thumbnailUrl: "https://salt.tikicdn.com/cache/280x280/ts/product/3e/15/a0/3bc86f3b2cc893e67ef1310f0ea27e3f.png",
                createdAt: now
            ),
            Build(
                name: "Bình Giữ Nhiệt Inox 304 Elmich EL8295 Dung Tích 500ml",
                slug: "binh-giu-nhiet-inox-304-elmich-el8295-dung-tich-500ml",
                description: "Bình giữ nhiệt cao cấp làm từ inox 304 an toàn thực phẩm, dung tích 500ml. Thiết kế hiện đại, giữ nhiệt lên đến 12 giờ, giữ lạnh đến 24 giờ. Thân bình chống trầy xước, nắp xoáy chắc chắn, phù hợp cho văn phòng và hoạt động ngoài trời. Có nhiều màu sắc để lựa chọn.",
                shortDescription: "Bình giữ nhiệt inox 304 Elmich EL8295 500ml, giữ nhiệt 12 giờ, nhiều màu.",
                skuNo: "6994556055959",
                price: 111720m,
                thumbnailUrl: "https://salt.tikicdn.com/cache/280x280/ts/product/b6/fe/b4/165dc71a83cea9aaaf58e002cd446722.jpg",
                createdAt: now
            ),
            Build(
                name: "Chảo Chống Dính Elmich EL5972 - EL5972GY (Xanh Mint)",
                slug: "chao-chong-dinh-elmich-el5972-xanh-mint",
                description: "Chảo chống dính Elmich EL5972 với lớp phủ chống dính cao cấp, màu xanh mint dịu mắt. Đế chảo dày, phân phối nhiệt đều, phù hợp với mọi loại bếp kể cả bếp từ. Tay cầm chống nóng, thiết kế nhẹ và dễ vệ sinh. Có 3 màu và 2 kích cỡ để lựa chọn.",
                shortDescription: "Chảo chống dính Elmich EL5972 xanh mint, dùng được bếp từ, 3 màu - 2 size.",
                skuNo: "9396908958144",
                price: 161120m,
                thumbnailUrl: "https://salt.tikicdn.com/cache/280x280/ts/product/5f/01/41/168c33fe4fd0e9c98ac747d3906c245a.jpg",
                createdAt: now
            ),
            Build(
                name: "Bình Giữ Nhiệt Gia Đình Inox 304 Elmich EL8299 Dung Tích 900ml - Hàng Chính Hãng",
                slug: "binh-giu-nhiet-gia-dinh-inox-304-elmich-el8299-dung-tich-900ml-hang-chinh-hang",
                description: "Bình giữ nhiệt cỡ lớn Elmich EL8299 dung tích 900ml, lý tưởng cho cả gia đình. Chất liệu inox 304 bền bỉ, không gỉ sét. Giữ nhiệt đến 18 giờ, giữ lạnh đến 36 giờ. Thiết kế tay cầm tiện lợi, nắp rót dễ sử dụng.",
                shortDescription: "Bình giữ nhiệt gia đình Elmich EL8299 900ml, inox 304, giữ nhiệt 18 giờ.",
                skuNo: "8600614472713",
                price: 290100m,
                thumbnailUrl: "https://salt.tikicdn.com/cache/280x280/ts/product/2c/41/54/b7d3d8acdcfa218a1180dfc271272788.png",
                createdAt: now
            ),
            Build(
                name: "Bộ Nồi Inox Dập Nguyên Khối Elmich Trimax Classic EL-2110OL Size 18, 20, 24, Chảo 26cm",
                slug: "bo-noi-inox-dap-nguyen-khoi-elmich-trimax-classic-el-2110ol-size-18-20-24-chao-26cm",
                description: "Bộ nồi inox cao cấp Elmich Trimax Classic dập nguyên khối gồm nồi size 18, 20, 24cm và chảo 26cm. Chất liệu inox 18/10 bền bỉ, đáy nồi 3 lớp phân phối nhiệt đều. Tương thích với mọi loại bếp, bao gồm bếp từ và lò nướng.",
                shortDescription: "Bộ nồi inox Elmich Trimax Classic, 4 món, đáy 3 lớp dùng được bếp từ.",
                skuNo: "2566430088910",
                price: 1718100m,
                thumbnailUrl: "https://salt.tikicdn.com/cache/280x280/media/catalog/producttmp/f7/83/26/63c82f21261db908c65d8e0dad707da1.png",
                createdAt: now
            ),
            Build(
                name: "Miếng Rửa Chén Bọt Biển Scrub Daddy Nguyên Bản - Miếng Xốp Lau Chùi Đa Năng",
                slug: "mieng-rua-chen-bot-bien-scrub-daddy-nguyen-ban-mieng-xop-lau-chui-da-nang",
                description: "Miếng rửa chén Scrub Daddy nổi tiếng thế giới với công nghệ FlexTexture độc quyền — cứng trong nước lạnh để cọ mạnh, mềm trong nước ấm để lau nhẹ. Hình mặt cười đáng yêu, không giữ mùi, không trầy xước bề mặt. Bền gấp 3 lần miếng bọt thường. Có 4 màu để lựa chọn.",
                shortDescription: "Miếng rửa chén Scrub Daddy FlexTexture, không mùi, không trầy xước, 4 màu.",
                skuNo: "7715635354376",
                price: 95200m,
                thumbnailUrl: "https://salt.tikicdn.com/cache/280x280/ts/product/f5/a4/76/b640154bcbead272f5785380fd6c0599.png",
                createdAt: now
            ),
            Build(
                name: "[NEW] Cốc Giữ Nhiệt Inox 304 Elmich EL8345 Dung Tích 480ML - Hàng Chính Hãng",
                slug: "new-coc-giu-nhiet-inox-304-elmich-el8345-dung-tich-480ml-hang-chinh-hang",
                description: "Phiên bản mới của cốc giữ nhiệt Elmich EL8345 dung tích 480ml với thiết kế tinh tế, cập nhật. Làm từ inox 304 cao cấp an toàn thực phẩm, lớp chân không giữ nhiệt/lạnh hiệu quả. Nắp có khóa an toàn, tiện lợi khi di chuyển. Có 3 màu sắc để lựa chọn.",
                shortDescription: "Cốc giữ nhiệt Elmich EL8345 480ml phiên bản mới, inox 304, 3 màu.",
                skuNo: "6400137289630",
                price: 102600m,
                thumbnailUrl: "https://salt.tikicdn.com/cache/280x280/ts/product/4e/a9/b9/ee2f1329eda07bff45b944761ca34ff4.jpg",
                createdAt: now
            ),
            Build(
                name: "Miếng Bọt Biển Lau Bụi Mọi Ngóc Ngách Damp Duster - Miếng Xốp Lau Chùi Đa Năng",
                slug: "mieng-bot-bien-lau-bui-moi-ngoc-ngach-damp-duster-mieng-xop-lau-chui-da-nang",
                description: "Miếng bọt biển Damp Duster đa năng, lý tưởng để lau sạch bụi bẩn trên mọi bề mặt — kính, gỗ, inox, nhựa. Chỉ cần làm ẩm nhẹ là có thể lau sạch mà không cần dùng hóa chất. Tiết kiệm, thân thiện môi trường và dễ vệ sinh tái sử dụng. Có 3 màu để lựa chọn.",
                shortDescription: "Miếng bọt biển Damp Duster đa năng, lau sạch không cần hóa chất, 3 màu.",
                skuNo: "1668242013483",
                price: 63200m,
                thumbnailUrl: "https://salt.tikicdn.com/cache/280x280/ts/product/a1/4a/88/ea52142a5f504d93b68b29c2869f4ba9.png",
                createdAt: now
            ),
            Build(
                name: "1 Gói Diệt Chuột Dạng Viên Hợp Trí Storm 0.005% - Gói 20 Viên",
                slug: "1-goi-diet-chuot-dang-vien-hop-tri-storm-0-005-goi-20-vien",
                description: "Thuốc diệt chuột Hợp Trí Storm dạng viên, hộp 20 viên. Thành phần hoạt chất Brodifacoum 0.005% thế hệ mới hiệu quả cao, chuột chỉ cần ăn 1 lần là có hiệu quả. An toàn khi sử dụng đúng hướng dẫn, ít ảnh hưởng đến vật nuôi và con người.",
                shortDescription: "Thuốc diệt chuột Storm 20 viên, Brodifacoum 0.005%, hiệu quả sau 1 lần.",
                skuNo: "1831551341333",
                price: 25600m,
                thumbnailUrl: "https://salt.tikicdn.com/cache/280x280/ts/product/03/4f/2d/653c61f3f5c0175d2ee4cb6502f94998.png",
                createdAt: now
            ),
            Build(
                name: "Màng Bọc Thực Phẩm PE Ringo - An Toàn, Dùng Được Trong Tủ Lạnh, Lò Vi Sóng, Có Lưỡi Cắt Dạng Trượt",
                slug: "mang-boc-thuc-pham-pe-ringo-an-toan-dung-duoc-trong-tu-lanh-lo-vi-song-co-luoi-cat-dang-truot",
                description: "Màng bọc thực phẩm Ringo làm từ nhựa PE an toàn, không chứa PVC hay chất độc hại. Độ bám dính cao, bọc kín thực phẩm, ngăn mùi và giữ độ tươi ngon lâu hơn. Dùng được trong tủ lạnh và lò vi sóng. Lưỡi cắt dạng trượt tiện lợi, dễ sử dụng hàng ngày. Có 2 size để lựa chọn.",
                shortDescription: "Màng bọc thực phẩm PE Ringo, an toàn, dùng được lò vi sóng, lưỡi cắt trượt.",
                skuNo: "7441106796389",
                price: 13600m,
                thumbnailUrl: "https://salt.tikicdn.com/cache/280x280/ts/product/18/be/5e/e46574c015e92545c691e36e11df2c5e.jpg",
                createdAt: now
            ),
        ];
    }

    private static Product Build(
        string name,
        string slug,
        string description,
        string shortDescription,
        string skuNo,
        decimal price,
        string thumbnailUrl,
        DateTimeOffset createdAt)
    {
        var categories = new List<ProductCategory> { new(HomeLivingCategoryId) };
        var images = new List<ProductImage> { new(0, thumbnailUrl) };

        var skus = new List<Sku>
        {
            new(skuNo, [], [new SkuImage(thumbnailUrl)], quantity: 100, isActive: true,
                price: new Money(price, Currency.VND))
        };

        var product = new Product(
            name:        name,
            description: description,
            status:      ProductStatus.Available,
            categories:  categories,
            attributes:  [],
            images:      images,
            skus:        skus,
            variants:    [],
            createdAt:   createdAt,
            updatedAt:   null,
            createdBy:   "seed",
            updatedBy:   null
        );

        typeof(Product).GetProperty("Slug")?.SetValue(product, slug);
        typeof(Product).GetProperty("ShortDescription")?.SetValue(product, shortDescription);
        typeof(Product).GetProperty("SellerId")?.SetValue(product, SeedSellerId);
        typeof(Product).GetProperty("Featured")?.SetValue(product, false);
        typeof(Product).GetProperty("Condition")?.SetValue(product, ProductCondition.New);

        return product;
    }
}
