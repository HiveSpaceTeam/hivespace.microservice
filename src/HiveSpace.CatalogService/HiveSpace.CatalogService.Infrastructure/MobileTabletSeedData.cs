using HiveSpace.CatalogService.Domain.Aggregates.ProductAggregate;
using HiveSpace.CatalogService.Domain.Common;
using HiveSpace.CatalogService.Domain.Enums;
using HiveSpace.CatalogService.Infrastructure.Data;
using HiveSpace.Domain.Shared.Enumerations;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace HiveSpace.CatalogService.Infrastructure;

public static class MobileTabletSeedData
{
    private static readonly Guid SeedSellerId = new("a1b2c3d4-e5f6-7890-abcd-ef1234567890");

    private const int MobileTabletCategoryId = 3;

    public static async Task SeedAsync(CatalogDbContext context, CancellationToken cancellationToken = default)
    {
        var anyExists = await context.Products
            .Where(p => p.Categories.Any(c => c.CategoryId == MobileTabletCategoryId))
            .AnyAsync(cancellationToken);
        if (anyExists)
        {
            Log.Debug("Điện Thoại - Máy Tính Bảng products already seeded. Skipping.");
            return;
        }

        // Load attribute ID map (name → id) so product attributes can reference them
        var attrMap = await context.Attributes
            .Where(a => a.ParentId != null)
            .ToDictionaryAsync(a => a.Name, a => a.Id, cancellationToken);

        var now = DateTimeOffset.UtcNow;
        var products = new List<Product>
        {
            BuildSamsungA075G(now, attrMap),
            Build("Điện Thoại Samsung Galaxy A26 5G (8GB/128GB) - Mặt Lưng Kính, AI Circle To Search, Camera HDR - Hàng Chính Hãng",
                  "dien-thoai-samsung-galaxy-a26-5g-8gb-128gb-mat-lung-kinh-ai-circle-to-search-camera-hdr-hang-chinh-hang",
                  "Samsung Galaxy A26 5G với mặt lưng kính sang trọng, chip Exynos 850 mạnh mẽ, hỗ trợ 5G tốc độ cao. Tính năng AI Circle to Search tìm kiếm thông tin ngay trên màn hình. Camera chính 50MP với HDR chụp đêm sáng rõ, pin 5000mAh dùng cả ngày. RAM 8GB, bộ nhớ trong 128GB.",
                  "Samsung Galaxy A26 5G, 8/128GB, mặt lưng kính, AI Circle to Search, camera HDR.",
                  "3255173313774", 5190000m,
                  "https://salt.tikicdn.com/cache/280x280/ts/product/11/c0/90/7ed78d65848ac95cac6cf6c03b6edcd0.jpg",
                  now),
            Build("Củ Sạc Samsung 25W Không Kèm Cáp - Hàng Chính Hãng",
                  "cu-sac-samsung-25w-khong-kem-cap-hang-chinh-hang",
                  "Củ sạc nhanh Samsung 25W chính hãng, tương thích với các dòng điện thoại Samsung Galaxy hỗ trợ sạc nhanh. Công nghệ Super Fast Charging 2.0 sạc đầy pin nhanh chóng. Đầu ra USB-C, kích thước nhỏ gọn tiện mang theo. Không kèm cáp.",
                  "Củ sạc nhanh Samsung 25W chính hãng, Super Fast Charging 2.0.",
                  "8464975672091", 220000m,
                  "https://salt.tikicdn.com/cache/280x280/ts/product/28/e4/7f/d1f6c5467529914727d0d213b507a0c6.png",
                  now),
            Build("Máy Tính Bảng Samsung Galaxy Tab S10 Lite WiFi (8GB/256GB) - Hàng Chính Hãng",
                  "may-tinh-bang-samsung-galaxy-tab-s10-lite-wifi-8gb-256gb-hang-chinh-hang",
                  "Samsung Galaxy Tab S10 Lite với màn hình Super AMOLED 10.1 inch sắc nét, chip Exynos 1380 hiệu năng cao. RAM 8GB, bộ nhớ trong 256GB lưu trữ thoải mái. Hỗ trợ bút S Pen sáng tạo không giới hạn. Kết nối WiFi 6, pin 8000mAh dùng lâu.",
                  "Samsung Galaxy Tab S10 Lite WiFi, 8/256GB, màn hình AMOLED, hỗ trợ S Pen.",
                  "2131434713241", 8590000m,
                  "https://salt.tikicdn.com/cache/280x280/ts/product/d9/f9/8f/a3627d1b345bb755d693e32e22320547.png",
                  now),
            Build("Điện Thoại Samsung Galaxy A37 5G (8GB/128GB) - Hàng Chính Hãng",
                  "dien-thoai-samsung-galaxy-a37-5g-8gb-128gb-hang-chinh-hang",
                  "Samsung Galaxy A37 5G với thiết kế hiện đại, màn hình Super AMOLED 6.6 inch 120Hz mượt mà. Chip Exynos 1380 mạnh mẽ, RAM 8GB, bộ nhớ 128GB. Camera chính 50MP chụp ảnh sắc nét trong mọi điều kiện ánh sáng.",
                  "Samsung Galaxy A37 5G, 8/128GB, màn hình 120Hz, camera 50MP.",
                  "6716358621390", 9890000m,
                  "https://salt.tikicdn.com/cache/280x280/ts/product/9b/28/00/9e79c4f0ec2aba5815d92182a6c17dab.jpg",
                  now),
            Build("Điện Thoại Samsung Galaxy A16 5G (4GB/128GB) - Đã Kích Hoạt Bảo Hành Điện Tử - Hàng Chính Hãng",
                  "dien-thoai-samsung-galaxy-a16-5g-4gb-128gb-da-kich-hoat-bao-hanh-dien-tu-hang-chinh-hang",
                  "Samsung Galaxy A16 5G phân khúc tầm trung với kết nối 5G hiện đại. Màn hình Super AMOLED 6.7 inch, tần số quét 90Hz. Chip MediaTek Dimensity 6300, RAM 4GB, bộ nhớ 128GB. Camera chính 50MP, pin 5000mAh sạc 25W.",
                  "Samsung Galaxy A16 5G, 4/128GB, màn hình 6.7 inch 90Hz, bảo hành chính hãng.",
                  "2802615525875", 4990000m,
                  "https://salt.tikicdn.com/cache/280x280/ts/product/1f/eb/e3/70ea444c1d20fd520feeb2950e11d95c.png",
                  now),
            Build("Apple iPhone 17e",
                  "apple-iphone-17e",
                  "iPhone 17e - mẫu iPhone mới nhất dòng e của Apple với chip A18 mạnh mẽ, hỗ trợ Apple Intelligence AI thế hệ mới. Màn hình Super Retina XDR 6.1 inch, camera hệ thống tiên tiến với chế độ chụp đêm và video 4K. Thiết kế nhôm aerospace cao cấp, kháng nước IP68.",
                  "iPhone 17e, chip A18, Apple Intelligence, màn hình 6.1 inch, kháng nước IP68.",
                  "8284368285277", 16990000m,
                  "https://salt.tikicdn.com/cache/280x280/ts/product/7b/51/47/5c76f01762ad765dec469005ab96833a.jpg",
                  now),
            Build("Điện Thoại Samsung Galaxy S26 Ultra (12GB/256GB) - Hàng Chính Hãng",
                  "dien-thoai-samsung-galaxy-s26-ultra-12gb-256gb-hang-chinh-hang",
                  "Samsung Galaxy S26 Ultra — đỉnh cao flagship của Samsung với chip Snapdragon 8 Elite thế hệ mới. Màn hình Dynamic AMOLED 2X 6.9 inch 120Hz cực sắc nét. Hệ thống camera 200MP với zoom quang học 10x. Tích hợp S Pen, RAM 12GB, bộ nhớ 256GB.",
                  "Samsung Galaxy S26 Ultra, 12/256GB, camera 200MP, zoom 10x, tích hợp S Pen.",
                  "5446043995267", 36990000m,
                  "https://salt.tikicdn.com/cache/280x280/ts/product/43/51/c7/591084ec0b8bbac62ec5458dadf251f8.jpg",
                  now),
            Build("Điện Thoại Samsung Galaxy S26 (12GB/256GB) - Hàng Chính Hãng",
                  "dien-thoai-samsung-galaxy-s26-12gb-256gb-hang-chinh-hang",
                  "Samsung Galaxy S26 flagship 2026 với chip Snapdragon 8 Elite mạnh mẽ. Màn hình Dynamic AMOLED 2X 6.2 inch 120Hz, thiết kế titanium sang trọng. Camera chính 50MP với AI nâng cao chất lượng ảnh. RAM 12GB, bộ nhớ 256GB, pin 4000mAh sạc nhanh 25W.",
                  "Samsung Galaxy S26, 12/256GB, chip Snapdragon 8 Elite, camera AI 50MP.",
                  "8235586884726", 25990000m,
                  "https://salt.tikicdn.com/cache/280x280/ts/product/8e/49/54/08c6ab66027620ef231fe7142c578536.jpg",
                  now),
        };

        await context.Products.AddRangeAsync(products, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        Log.Debug("Seeded {Count} products for Điện Thoại - Máy Tính Bảng.", products.Count);
    }

    private static Product BuildSamsungA075G(DateTimeOffset now, Dictionary<string, int> attrMap)
    {
        var categories = new List<ProductCategory> { new(MobileTabletCategoryId) };

        var images = new List<ProductImage>
        {
            new(0, "https://salt.tikicdn.com/ts/product/9b/6f/bd/ee72cd139b8b14c013092590ea91438f.jpg"),
            new(0, "https://salt.tikicdn.com/ts/product/91/f3/8e/717c0b646c570e23fec95c2f80d1a535.jpg"),
            new(0, "https://salt.tikicdn.com/ts/product/0d/03/5f/d2b828ee70dd66e8af089a7339a76b31.jpg"),
            new(0, "https://salt.tikicdn.com/ts/product/56/6f/02/2d5a6985eabd2064a84061d91cf316af.jpg"),
            new(0, "https://salt.tikicdn.com/ts/product/2d/f5/6a/3f36ef67c8b7b09bf1101c93432f90e7.jpg"),
            new(0, "https://salt.tikicdn.com/ts/product/0f/f7/3e/4fe3d6f26fc19517888356c74920dea2.jpg"),
            new(0, "https://salt.tikicdn.com/ts/product/fc/db/77/32c651c480a83436210627daf5ec14ae.jpg"),
            new(0, "https://salt.tikicdn.com/ts/product/32/fa/b2/7f23142a09143367e92aabf6052d20af.jpg"),
        };

        var variantMau = new ProductVariant("Màu");
        variantMau.AddOption("Đen");
        variantMau.AddOption("Xanh");

        var variantDungLuong = new ProductVariant("Dung lượng");
        variantDungLuong.AddOption("(4GB/128GB)");

        var variants = new List<ProductVariant> { variantMau, variantDungLuong };

        var skuDen = new Sku(
            skuNo: "9677706341374",
            skuVariants:
            [
                new SkuVariant("Màu", "Đen"),
                new SkuVariant("Dung lượng", "(4GB/128GB)"),
            ],
            images:
            [
                new SkuImage("https://salt.tikicdn.com/ts/product/9b/6f/bd/ee72cd139b8b14c013092590ea91438f.jpg"),
                new SkuImage("https://salt.tikicdn.com/ts/product/91/f3/8e/717c0b646c570e23fec95c2f80d1a535.jpg"),
                new SkuImage("https://salt.tikicdn.com/ts/product/0d/03/5f/d2b828ee70dd66e8af089a7339a76b31.jpg"),
                new SkuImage("https://salt.tikicdn.com/ts/product/56/6f/02/2d5a6985eabd2064a84061d91cf316af.jpg"),
                new SkuImage("https://salt.tikicdn.com/ts/product/2d/f5/6a/3f36ef67c8b7b09bf1101c93432f90e7.jpg"),
                new SkuImage("https://salt.tikicdn.com/ts/product/0f/f7/3e/4fe3d6f26fc19517888356c74920dea2.jpg"),
                new SkuImage("https://salt.tikicdn.com/ts/product/fc/db/77/32c651c480a83436210627daf5ec14ae.jpg"),
                new SkuImage("https://salt.tikicdn.com/ts/product/32/fa/b2/7f23142a09143367e92aabf6052d20af.jpg"),
            ],
            quantity: 100, isActive: true,
            price: new Money(3590000m, Currency.VND)
        );

        var skuXanh = new Sku(
            skuNo: "7422409007473",
            skuVariants:
            [
                new SkuVariant("Màu", "Xanh"),
                new SkuVariant("Dung lượng", "(4GB/128GB)"),
            ],
            images:
            [
                new SkuImage("https://salt.tikicdn.com/ts/product/0d/03/5f/077cf1936d239085c90b1ddf633fe615.jpg"),
                new SkuImage("https://salt.tikicdn.com/ts/product/0f/f7/3e/296cd1ade373589960c36615851fc06a.jpg"),
                new SkuImage("https://salt.tikicdn.com/ts/product/2d/f5/6a/8ad175740cb4c5a7b56689ff58b1ae4f.jpg"),
                new SkuImage("https://salt.tikicdn.com/ts/product/32/fa/b2/b45c682e7e50ab0408ae742b298828ca.jpg"),
                new SkuImage("https://salt.tikicdn.com/ts/product/56/6f/02/e9b3559f843eb8babab9315a4e55af58.jpg"),
                new SkuImage("https://salt.tikicdn.com/ts/product/71/34/e7/1efcd5d7e1fe88396fd9f3e76eacc6d2.jpg"),
                new SkuImage("https://salt.tikicdn.com/ts/product/91/f3/8e/02560996623fece58cb7e746a7e03e3e.jpg"),
                new SkuImage("https://salt.tikicdn.com/ts/product/fc/db/77/61f738ea26e0de623a538ee83f54bc29.jpg"),
            ],
            quantity: 100, isActive: true,
            price: new Money(3590000m, Currency.VND)
        );

        // ProductAttributes — map Tiki warranty_info + specifications into existing Attribute system
        var attributes = new List<ProductAttribute>();
        void AddAttr(string name, string value)
        {
            if (attrMap.TryGetValue(name, out var id))
                attributes.Add(new ProductAttribute(id, freeTextValue: value));
        }

        // warranty_info
        AddAttr("Thời gian bảo hành", "12 Tháng");
        AddAttr("Hình thức bảo hành", "Điện tử");
        AddAttr("Nơi bảo hành",       "Bảo hành chính hãng");

        // specifications
        AddAttr("Thương hiệu",       "Samsung");
        AddAttr("Xuất xứ (Made in)", "Việt Nam");
        AddAttr("Có thuế VAT",       "Có");
        AddAttr("Hệ điều hành",      "Android 14");
        AddAttr("Kích thước màn hình", "6.5 inch");
        AddAttr("Dung lượng pin",    "5000 mAh");
        AddAttr("Loại màn hình",     "LCD");
        AddAttr("Camera trước",      "8 MP");
        AddAttr("Camera sau",        "48 MP + 2 MP + 2 MP");
        AddAttr("Chip xử lý (CPU)",  "MediaTek Dimensity 6100+");

        const string description = """
            <p><img src="https://salt.tikicdn.com/ts/tmp/c0/84/8f/5de0776d37bba46053146055aff2661f.jpg" alt="" width="750" height="421" /></p>
            <p><img src="https://salt.tikicdn.com/ts/tmp/27/96/fe/13b8669ed63f949506c5c8b6178d001e.png" alt="" width="750" height="375" /></p>
            <p><img src="https://salt.tikicdn.com/ts/tmp/25/42/93/136e636afc6637f61f14a06b80d8942b.png" alt="" width="750" height="375" /></p>
            <p><img src="https://salt.tikicdn.com/ts/tmp/22/ff/71/39eeef8fb4e652341c0a4e8f7fe4c233.png" alt="" width="750" height="375" /></p>
            <p><img src="https://salt.tikicdn.com/ts/tmp/e7/75/33/923bbff0597d9cd3a82c0f8c7df300ad.png" alt="" width="750" height="375" /></p>
            <p><img src="https://salt.tikicdn.com/ts/tmp/30/1d/e1/c76b36b25de5c20362bfb6f91046eabd.png" alt="" width="750" height="375" /></p>
            <p><img src="https://salt.tikicdn.com/ts/tmp/a8/36/3d/20a8a3def2f98e038dd4b2360031d17b.jpg" alt="" width="750" height="381" /></p>
            """;

        var product = new Product(
            name: "Điện Thoại Samsung Galaxy A07 5G - Hàng Chính Hãng",
            description: description,
            status: ProductStatus.Available,
            categories: categories,
            attributes: attributes,
            images: images,
            skus: [skuDen, skuXanh],
            variants: variants,
            createdAt: now,
            updatedAt: null,
            createdBy: "seed",
            updatedBy: null
        );

        typeof(Product).GetProperty("Slug")?.SetValue(product, "dien-thoai-samsung-galaxy-a07-5g-hang-chinh-hang");
        typeof(Product).GetProperty("ShortDescription")?.SetValue(product, "Samsung Galaxy A07 5G, màn hình 6.5 inch, camera 48MP, pin 5000mAh.");
        typeof(Product).GetProperty("SellerId")?.SetValue(product, SeedSellerId);
        typeof(Product).GetProperty("Featured")?.SetValue(product, false);
        typeof(Product).GetProperty("Condition")?.SetValue(product, ProductCondition.New);

        return product;
    }

    private static Product Build(
        string name, string slug, string description, string shortDescription,
        string skuNo, decimal price, string thumbnailUrl, DateTimeOffset createdAt)
    {
        var categories = new List<ProductCategory> { new(MobileTabletCategoryId) };
        var images = new List<ProductImage> { new(0, thumbnailUrl) };
        var skus = new List<Sku>
        {
            new(skuNo, [], [new SkuImage(thumbnailUrl)], quantity: 100, isActive: true,
                price: new Money(price, Currency.VND))
        };

        var product = new Product(
            name: name, description: description, status: ProductStatus.Available,
            categories: categories, attributes: [], images: images, skus: skus, variants: [],
            createdAt: createdAt, updatedAt: null, createdBy: "seed", updatedBy: null
        );

        typeof(Product).GetProperty("Slug")?.SetValue(product, slug);
        typeof(Product).GetProperty("ShortDescription")?.SetValue(product, shortDescription);
        typeof(Product).GetProperty("SellerId")?.SetValue(product, SeedSellerId);
        typeof(Product).GetProperty("Featured")?.SetValue(product, false);
        typeof(Product).GetProperty("Condition")?.SetValue(product, ProductCondition.New);

        return product;
    }
}
