using HiveSpace.CatalogService.Domain.Aggregates.ProductAggregate;
using HiveSpace.Domain.Shared.ValueObjects;
using HiveSpace.CatalogService.Domain.Enums;
using HiveSpace.CatalogService.Infrastructure.Data;
using HiveSpace.Domain.Shared.Enumerations;
using HiveSpace.Infrastructure.Persistence.Seeding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HiveSpace.CatalogService.Infrastructure.SeedData;

internal sealed class MobileTabletSeeder(CatalogDbContext db, ILogger<MobileTabletSeeder> logger) : ISeeder
{
    public int Order => 6;

    public async Task SeedAsync(CancellationToken ct = default)
    {
        var categoryMap = await db.Categories
            .ToDictionaryAsync(c => c.Name, c => c.Id, ct);

        if (!categoryMap.TryGetValue("Điện Thoại - Máy Tính Bảng", out var categoryId))
        {
            logger.LogWarning("Category 'Điện Thoại - Máy Tính Bảng' not found. Skipping MobileTabletSeeder.");
            return;
        }

        var anyExists = await db.Products
            .Where(p => p.Categories.Any(c => c.CategoryId == categoryId))
            .AnyAsync(ct);

        if (anyExists)
        {
            logger.LogDebug("Điện Thoại - Máy Tính Bảng products already seeded. Skipping.");
            return;
        }

        var attrMap = await db.Attributes
            .Where(a => a.ParentId != null)
            .ToDictionaryAsync(a => a.Name, a => a.Id, ct);

        var now = DateTimeOffset.UtcNow;
        var products = new List<Product>
        {
            BuildSamsungA075G(now, attrMap, categoryId),
            BuildSamsungA265G(now, attrMap, categoryId),
            BuildCuSacSamsung25W(now, attrMap, categoryId),
            BuildTabA11(now, attrMap, categoryId),
            BuildSamsungA375G(now, attrMap, categoryId),
            BuildSamsungA165G(now, attrMap, categoryId),
            BuildIPhone17e(now, attrMap, categoryId),
            BuildSamsungS26Ultra(now, attrMap, categoryId),
            BuildSamsungS26(now, attrMap, categoryId),
        };

        var strategy = db.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await using var tx = await db.Database.BeginTransactionAsync(ct);
            await db.Products.AddRangeAsync(products, ct);
            await db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
        });
        logger.LogInformation("Seeded {Count} products for Điện Thoại - Máy Tính Bảng.", products.Count);
    }

    // ── Product 1: Samsung Galaxy A07 5G ─────────────────────────────────────────
    private static Product BuildSamsungA075G(DateTimeOffset now, Dictionary<string, int> attrMap, int categoryId)
    {
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
            price: Money.FromVND((long)(3590000))
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
            price: Money.FromVND((long)(3590000))
        );

        var attributes = BuildMobileAttrs(attrMap,
            thuongHieu:   "Samsung",
            xuatXu:       "Việt Nam",
            dungLuongPin: "5000 mAh",
            manHinh:      "LCD",
            cameraSau:    "48 MP + 2 MP + 2 MP",
            cameraTruoc:  "8 MP",
            cpu:          "MediaTek Dimensity 6100+");

        void AddWarranty(string name, string value)
        {
            if (!attrMap.TryGetValue(name, out var id))
                throw new InvalidOperationException(
                    $"Attribute '{name}' not found. Ensure AttributeSeeder ran before product seeders.");
            attributes.Add(new ProductAttribute(id, freeTextValue: value));
        }
        AddWarranty("Thời gian bảo hành", "12 Tháng");
        AddWarranty("Hình thức bảo hành", "Điện tử");
        AddWarranty("Nơi bảo hành",       "Bảo hành chính hãng");
        AddWarranty("Có thuế VAT",        "Có");
        AddWarranty("Kích thước màn hình", "6.5 inch");
        AddWarranty("Hệ điều hành",       "Android 14");

        const string description = """
            <p><img src="https://salt.tikicdn.com/ts/tmp/c0/84/8f/5de0776d37bba46053146055aff2661f.jpg" alt="" width="750" height="421" /></p>
            <p><img src="https://salt.tikicdn.com/ts/tmp/27/96/fe/13b8669ed63f949506c5c8b6178d001e.png" alt="" width="750" height="375" /></p>
            <p><img src="https://salt.tikicdn.com/ts/tmp/25/42/93/136e636afc6637f61f14a06b80d8942b.png" alt="" width="750" height="375" /></p>
            <p><img src="https://salt.tikicdn.com/ts/tmp/22/ff/71/39eeef8fb4e652341c0a4e8f7fe4c233.png" alt="" width="750" height="375" /></p>
            <p><img src="https://salt.tikicdn.com/ts/tmp/e7/75/33/923bbff0597d9cd3a82c0f8c7df300ad.png" alt="" width="750" height="375" /></p>
            <p><img src="https://salt.tikicdn.com/ts/tmp/30/1d/e1/c76b36b25de5c20362bfb6f91046eabd.png" alt="" width="750" height="375" /></p>
            <p><img src="https://salt.tikicdn.com/ts/tmp/a8/36/3d/20a8a3def2f98e038dd4b2360031d17b.jpg" alt="" width="750" height="381" /></p>
            """;

        return BuildFull(
            "Điện Thoại Samsung Galaxy A07 5G - Hàng Chính Hãng",
            "dien-thoai-samsung-galaxy-a07-5g-hang-chinh-hang",
            "Samsung Galaxy A07 5G, màn hình 6.5 inch, camera 48MP, pin 5000mAh.",
            images, attributes, [skuDen, skuXanh], variants, description, now, categoryId);
    }

    // ── Product 2: Samsung Galaxy A26 5G ─────────────────────────────────────────
    private static Product BuildSamsungA265G(DateTimeOffset now, Dictionary<string, int> attrMap, int categoryId)
    {
        var images = new List<ProductImage>
        {
            new(0, "https://salt.tikicdn.com/ts/product/11/c0/90/7ed78d65848ac95cac6cf6c03b6edcd0.jpg"),
            new(0, "https://salt.tikicdn.com/ts/product/67/f8/e8/fbb70dd6df47766d78997dbecdd97cc2.jpg"),
            new(0, "https://salt.tikicdn.com/ts/product/bb/1e/5e/63e18eaf0a2d236b84ce79099043c325.jpg"),
            new(0, "https://salt.tikicdn.com/ts/product/6e/fe/be/1b917108eb41ff90f919c2780aa05562.jpg"),
            new(0, "https://salt.tikicdn.com/ts/product/cd/99/4e/02b5a0f62c1f78d5dbece58c11e0f6ce.jpg"),
            new(0, "https://salt.tikicdn.com/ts/product/b3/ea/f3/ed03a533802ebff6328386ff62af3e97.jpg"),
            new(0, "https://salt.tikicdn.com/ts/product/fe/d6/39/981ca89dd384baa6747498b0ca0637e9.jpg"),
            new(0, "https://salt.tikicdn.com/ts/product/28/a5/84/f83014b87adc37730b4878a830b75cb3.jpg"),
        };

        var attributes = BuildMobileAttrs(attrMap,
            thuongHieu: "Samsung", xuatXu: "Việt Nam", dungLuongPin: "5000 mAh",
            manHinh: "Super AMOLED", cameraSau: "50+8+2MP", cameraTruoc: "13MP",
            cpu: "Exynos 1380 (Quartz)");

        var variantMau = new ProductVariant("Màu");
        variantMau.AddOption("Xanh"); variantMau.AddOption("Đen"); variantMau.AddOption("Cam");
        var variants = new List<ProductVariant> { variantMau };

        var skuXanh = new Sku("4276090513087",
            [new SkuVariant("Màu", "Xanh")],
            [new SkuImage("https://salt.tikicdn.com/ts/product/11/c0/90/7ed78d65848ac95cac6cf6c03b6edcd0.jpg")],
            quantity: 100, isActive: true, price: Money.FromVND((long)(6990000)));
        var skuDen = new Sku("4937698595186",
            [new SkuVariant("Màu", "Đen")],
            [new SkuImage("https://salt.tikicdn.com/ts/product/6d/5f/6c/a8a727e8a5e3bc04f997f4394b44a675.jpg")],
            quantity: 100, isActive: true, price: Money.FromVND((long)(6990000)));
        var skuCam = new Sku("5458430373072",
            [new SkuVariant("Màu", "Cam")],
            [new SkuImage("https://salt.tikicdn.com/ts/product/22/d5/e5/aa6d5a53a27f34a634ee0b51cd64a3f0.jpg")],
            quantity: 100, isActive: true, price: Money.FromVND((long)(6990000)));

        const string description = """
            <p><strong>Điện thoại Samsung Galaxy A26 5G (8/128GB) - </strong><br /><strong>Awesome Intelligence - Circle to Search - Chụp nét tìm nhanh, bắt trend càng xịn</strong></p>
            <p><img src="https://salt.tikicdn.com/ts/tmp/c8/d2/1e/dc6328ed461c6effec9f09d444fdf46e.jpg" alt="" width="750" height="156" /></p>
            <p><img src="https://salt.tikicdn.com/ts/tmp/d4/46/78/3e327344936becea58acb9fbe97557fd.jpg" alt="" width="750" height="360" /></p>
            <p><img src="https://salt.tikicdn.com/ts/tmp/12/69/7a/a10ac76d93c0f9eba21ebc64971a4eca.jpg" alt="" width="750" height="287" /></p>
            <p><img src="https://salt.tikicdn.com/ts/tmp/96/60/28/cf6f3519953019bdc2b130bb5a14e9af.jpg" alt="" width="750" height="287" /></p>
            <p><img src="https://salt.tikicdn.com/ts/tmp/df/6a/43/5b67cec228ff43ab22778d101eeb6817.jpg" alt="" width="750" height="287" /></p>
            <p><img src="https://salt.tikicdn.com/ts/tmp/c8/b2/ab/ed088753ee2daf82b82b59c667e3470f.jpg" alt="" width="750" height="287" /></p>
            <p><img src="https://salt.tikicdn.com/ts/tmp/f0/fc/f9/b2d910c017e55c93793cbfd27a4d47c3.jpg" alt="" width="750" height="306" /></p>
            <p><img src="https://salt.tikicdn.com/ts/tmp/2f/85/b0/30b115b22248649722dcfd6553249bd4.jpg" alt="" width="750" height="287" /></p>
            <p><img src="https://salt.tikicdn.com/ts/tmp/fc/e8/c2/e2dc0eefe9074efb623dece20a02e098.jpg" alt="" width="750" height="287" /></p>
            <p><strong>Thông số kỹ thuật</strong><br /><br /><strong>Màn Hình</strong><br />- Công nghệ màn hình: Super AMOLED<br />- Độ phân giải: 1080 x 2340 (FHD+)<br />- Kích thước: 6.7 inch<br />- Tần số quét: 120Hz<br /><br /><strong>Camera sau</strong><br />- Độ phân giải: 50+8+2MP<br /><br /><strong>Camera trước</strong><br />- Độ phân giải: 13MP<br /><br /><strong>Pin và sạc</strong><br />- Dung lượng pin: 5000 mAh<br />- Hỗ trợ sạc tối đa: 25W</p>
            """;

        return BuildFull("Điện thoại Samsung Galaxy A26 5G (8/128GB), Mặt lưng kính, AI-Circle to Search, Camera HDR chụp đêm sáng rõ - Hàng chính hãng",
            "dien-thoai-samsung-galaxy-a26-5g-8gb-128gb-mat-lung-kinh-ai-circle-to-search-camera-hdr-hang-chinh-hang",
            "Samsung Galaxy A26 5G, 8/128GB, mặt lưng kính, AI Circle to Search, camera HDR.",
            images, attributes, [skuXanh, skuDen, skuCam], variants, description, now, categoryId);
    }

    // ── Product 3: Củ Sạc Samsung 25W ────────────────────────────────────────────
    private static Product BuildCuSacSamsung25W(DateTimeOffset now, Dictionary<string, int> attrMap, int categoryId)
    {
        var images = new List<ProductImage>
        {
            new(0, "https://salt.tikicdn.com/ts/product/28/e4/7f/d1f6c5467529914727d0d213b507a0c6.png"),
            new(0, "https://salt.tikicdn.com/ts/product/f3/f8/f0/5b7f99b8b4aa0cef5f396a764f083714.png"),
            new(0, "https://salt.tikicdn.com/ts/product/86/29/9d/5d984d6d47372f5163ea95fc5e744d1b.png"),
            new(0, "https://salt.tikicdn.com/ts/product/55/65/6d/ce683e7ac70db1243c4984fcb0a5dfa7.png"),
            new(0, "https://salt.tikicdn.com/ts/product/e8/da/3d/08cd3e0f9fbf1008a8d01ef3840e8e28.png"),
        };

        var attributes = BuildMobileAttrs(attrMap, thuongHieu: "Samsung", xuatXu: "Trung Quốc");

        var skuTrang = new Sku("7768310632659",
            [new SkuVariant("Màu", "Trắng")],
            [new SkuImage("https://salt.tikicdn.com/ts/product/28/e4/7f/d1f6c5467529914727d0d213b507a0c6.png")],
            quantity: 100, isActive: true, price: Money.FromVND((long)(540000)));

        var variantMau = new ProductVariant("Màu");
        variantMau.AddOption("Trắng");
        var variants = new List<ProductVariant> { variantMau };

        const string description = """
            <h2>Mạnh mẽ, tinh tế</h2>
            <p>Củ sạc nhanh mới với tính năng Sạc Siêu Nhanh 25W, dành cho thiết bị tương thích với cáp sạc USB Type-C.</p>
            <p>*Sạc siêu nhanh (SFC) là công nghệ sạc của Samsung dựa trên Power Delivery 3.0.</p>
            <h2>Nhỏ gọn mà mạnh mẽ</h2>
            <p>Nhờ Công nghệ sạc GaN mà giờ đây củ sạc đã trở nên nhỏ gọn hơn thuận tiện cho du lịch. Sản phẩm với hai phiên bản màu trắng và màu đen.</p>
            <h2>Giảm thiểu năng lượng khi ở chế độ sạc chờ</h2>
            <p>Khi ở chế độ chờ sạc, củ sạc sẽ giảm thiểu nguồn năng lượng tiêu thụ từ 20mW xuống 5mW, giúp tiết kiệm năng lượng lên đến 75%.</p>
            <h2>Sạc nhanh và An toàn</h2>
            <p>An toàn là trên hết. Không còn nỗi lo dòng điện quá tải cho phép, đoản mạch, nhiệt độ cao, rò rỉ điện,…</p>
            <h2>Tương thích với Type-C</h2>
            <p>Với công nghệ sạc nhanh lên đến 25W, những thiết bị tương thích với Type-C của bạn sẽ nhanh chóng được sạc đầy.</p>
            """;

        return BuildFull("Củ Sạc Samsung Không Kèm Cáp 25W - Hàng Chính Hãng",
            "cu-sac-samsung-khong-kem-cap-25w-hang-chinh-hang",
            "Củ sạc nhanh Samsung 25W chính hãng, công nghệ GaN, Super Fast Charging.",
            images, attributes, [skuTrang], variants, description, now, categoryId);
    }

    // ── Product 4: Samsung Galaxy Tab A11 WiFi ────────────────────────────────────
    private static Product BuildTabA11(DateTimeOffset now, Dictionary<string, int> attrMap, int categoryId)
    {
        var images = new List<ProductImage>
        {
            new(0, "https://salt.tikicdn.com/ts/product/c8/0d/8a/eca6b95e78cbeb16863a2bedbf8f5691.png"),
            new(0, "https://salt.tikicdn.com/ts/product/9f/7f/31/12934670f798eea57abd8023527eb2e6.jpg"),
            new(0, "https://salt.tikicdn.com/ts/product/4b/ed/e2/ed7d8b4403fe9c2a2a30a4c8c9dcd61d.jpg"),
            new(0, "https://salt.tikicdn.com/ts/product/f5/3a/68/18cbf01b4765c635329659f98a72a08a.jpg"),
            new(0, "https://salt.tikicdn.com/ts/product/5c/03/95/e53328b037b275a0f911445da6e6ce2b.jpg"),
            new(0, "https://salt.tikicdn.com/ts/product/51/9b/00/3388421585de1e7178d8d5817d7933f4.jpg"),
        };

        var attributes = BuildMobileAttrs(attrMap,
            thuongHieu: "Samsung", xuatXu: "Trung Quốc", dungLuongPin: "5100 mAh",
            cameraSau: "8 MP", cameraTruoc: "5 MP", cpu: "MediaTek Helio G99");

        var skuBac = new Sku("1517847354506",
            [new SkuVariant("Màu sắc", "Bạc")],
            [new SkuImage("https://salt.tikicdn.com/ts/product/c8/0d/8a/eca6b95e78cbeb16863a2bedbf8f5691.png")],
            quantity: 100, isActive: true, price: Money.FromVND((long)(3990000)));

        var variantMau = new ProductVariant("Màu sắc");
        variantMau.AddOption("Bạc");
        var variants = new List<ProductVariant> { variantMau };

        const string description = """
            <p>Máy Tính Bảng Samsung Galaxy Tab A11 WiFi 4GB/64GB</p>
            <p>Màn hình 11 inch, chip MediaTek Helio G99, RAM 4GB, bộ nhớ 64GB mở rộng lên 2TB qua MicroSD. Pin 5100 mAh, camera sau 8MP, camera trước 5MP. WiFi 5 Dual-band, Bluetooth v5.3, cổng sạc Type-C. Đã kích hoạt bảo hành điện tử.</p>
            """;

        return BuildFull("Máy Tính Bảng Samsung Galaxy Tab A11 WiFi 4GB/64GB - Đã Kích Hoạt Bảo Hành Điện Tử - Hàng Chính Hãng",
            "may-tinh-bang-samsung-galaxy-tab-a11-wifi-4gb-64gb-da-kich-hoat-bao-hanh-dien-tu-hang-chinh-hang",
            "Samsung Galaxy Tab A11 WiFi, 4/64GB, màn hình 11 inch, chip Helio G99.",
            images, attributes, [skuBac], variants, description, now, categoryId);
    }

    // ── Product 5: Samsung Galaxy A37 5G ─────────────────────────────────────────
    private static Product BuildSamsungA375G(DateTimeOffset now, Dictionary<string, int> attrMap, int categoryId)
    {
        var images = new List<ProductImage>
        {
            new(0, "https://salt.tikicdn.com/ts/product/ff/10/ae/26b65b7e9ddae0c4e4514dd5d0ef7ef2.jpg"),
            new(0, "https://salt.tikicdn.com/ts/product/72/39/fe/f89be2d05df88b6db2c0a0484d25c764.jpg"),
            new(0, "https://salt.tikicdn.com/ts/product/f6/7f/22/d83cce51f3bf5034217bbeddf905889d.jpg"),
            new(0, "https://salt.tikicdn.com/ts/product/82/8c/ea/15faaa372eb980f1aad44d343c47556c.jpg"),
            new(0, "https://salt.tikicdn.com/ts/product/cb/41/18/015cf71706aa4c833e7d0cdfadab0876.jpg"),
            new(0, "https://salt.tikicdn.com/ts/product/5a/6d/77/da24af5af11b54d469f98f3668aed1e6.jpg"),
            new(0, "https://salt.tikicdn.com/ts/product/6d/64/b3/f1b09d1dda269002d538ad4aa62d9f96.jpg"),
        };

        var attributes = BuildMobileAttrs(attrMap, thuongHieu: "Samsung", xuatXu: "Việt Nam");

        var variantMau = new ProductVariant("Màu sắc");
        variantMau.AddOption("Xanh"); variantMau.AddOption("Đen");
        variantMau.AddOption("Trắng"); variantMau.AddOption("Tím");
        var variants = new List<ProductVariant> { variantMau };

        var skuXanh = new Sku("1807041004396",
            [new SkuVariant("Màu sắc", "Xanh")],
            [new SkuImage("https://salt.tikicdn.com/ts/product/ff/10/ae/26b65b7e9ddae0c4e4514dd5d0ef7ef2.jpg")],
            quantity: 100, isActive: true, price: Money.FromVND((long)(10990000)));
        var skuDen = new Sku("6116779689074",
            [new SkuVariant("Màu sắc", "Đen")],
            [new SkuImage("https://salt.tikicdn.com/ts/product/6a/63/e2/b5cacfcd9cd46b01acf952837fcb09b8.jpg")],
            quantity: 100, isActive: true, price: Money.FromVND((long)(10990000)));
        var skuTrang = new Sku("7416742245238",
            [new SkuVariant("Màu sắc", "Trắng")],
            [new SkuImage("https://salt.tikicdn.com/ts/product/9b/28/00/9e79c4f0ec2aba5815d92182a6c17dab.jpg")],
            quantity: 100, isActive: true, price: Money.FromVND((long)(10990000)));
        var skuTim = new Sku("3862966111725",
            [new SkuVariant("Màu sắc", "Tím")],
            [new SkuImage("https://salt.tikicdn.com/ts/product/ce/7f/e3/60dd7a468610770f492349f10c8f6962.jpg")],
            quantity: 100, isActive: true, price: Money.FromVND((long)(10990000)));

        const string description = """
            <p><img src="https://salt.tikicdn.com/ts/tmp/62/78/46/9b4ac604c207e3d0e646dc97fb5cb944.png" alt="" width="750" height="392" /></p>
            <p><img src="https://salt.tikicdn.com/ts/tmp/7b/bb/51/aaacf77e9538c3b4cbbbd9c0efe20a01.png" alt="" width="750" height="311" /></p>
            <p><img src="https://salt.tikicdn.com/ts/tmp/6b/c5/da/a204989a5ecb5e92915d3af359d2c703.png" alt="" width="750" height="311" /></p>
            <p><img src="https://salt.tikicdn.com/ts/tmp/e8/59/83/aee50d5ba1ffd790a144e9020d97acc2.png" alt="" width="750" height="311" /></p>
            <p><img src="https://salt.tikicdn.com/ts/tmp/3f/94/03/c34eb515fc47009595bcad29a2cafda7.png" alt="" width="750" height="311" /></p>
            <p><img src="https://salt.tikicdn.com/ts/tmp/64/07/62/9251c83f65c9724a65de70599717233e.png" alt="" width="750" height="311" /></p>
            <p><img src="https://salt.tikicdn.com/ts/tmp/31/c3/78/ff19bf39e6a95299df179fd02244bb16.png" alt="" width="750" height="311" /></p>
            <p><img src="https://salt.tikicdn.com/ts/tmp/18/bf/29/cf20f7ba5151b4a10597d5731f547fd4.png" alt="" width="750" height="311" /></p>
            <p><img src="https://salt.tikicdn.com/ts/tmp/5f/89/fc/d4a93ed17dbdd4ecf78096464e2c5caf.png" alt="" width="750" height="311" /></p>
            <p><img src="https://salt.tikicdn.com/ts/tmp/96/79/92/c4e09e47643a41c4b707fa7579d18dfe.png" alt="" width="750" height="311" /></p>
            <p><img src="https://salt.tikicdn.com/ts/tmp/9c/e7/c8/6d2362034d8abb1938d8e7e0d31bbd86.jpg" alt="" width="750" height="392" /></p>
            <p><img src="https://salt.tikicdn.com/ts/tmp/87/2e/b7/32c8aff558faf43f1588432a4508a85d.jpg" alt="" width="750" height="392" /></p>
            """;

        return BuildFull("Điện Thoại Samsung Galaxy A37 5G (8GB/128GB) - Hàng Chính Hãng",
            "dien-thoai-samsung-galaxy-a37-5g-8gb-128gb-hang-chinh-hang",
            "Samsung Galaxy A37 5G, 8/128GB, 4 màu, hàng chính hãng.",
            images, attributes, [skuXanh, skuDen, skuTrang, skuTim], variants, description, now, categoryId);
    }

    // ── Product 6: Samsung Galaxy A16 5G ─────────────────────────────────────────
    private static Product BuildSamsungA165G(DateTimeOffset now, Dictionary<string, int> attrMap, int categoryId)
    {
        var images = new List<ProductImage>
        {
            new(0, "https://salt.tikicdn.com/ts/product/1f/eb/e3/70ea444c1d20fd520feeb2950e11d95c.png"),
            new(0, "https://salt.tikicdn.com/ts/product/be/7d/86/bdbac325f0d9f99d1ef2304de2c89631.png"),
            new(0, "https://salt.tikicdn.com/ts/product/27/53/e7/9b9a08c38592d279db900896faba507d.jpg"),
            new(0, "https://salt.tikicdn.com/ts/product/03/9f/45/6d4699c2a0d2c0146a901956209541f2.jpg"),
            new(0, "https://salt.tikicdn.com/ts/product/ae/8d/3f/e144f48792fb44a46ae9eec270e7ecc5.jpg"),
            new(0, "https://salt.tikicdn.com/ts/product/36/2a/cd/9596b994d17b4da3f6f4c28debc1bc75.jpg"),
            new(0, "https://salt.tikicdn.com/ts/product/97/0d/c1/95c05eafe1a24e6418c31b75b8e07a9e.jpg"),
            new(0, "https://salt.tikicdn.com/ts/product/85/cc/7e/7da0dae588b76c6e904c93636e2c2e2a.jpg"),
        };

        var attributes = BuildMobileAttrs(attrMap, thuongHieu: "Samsung", xuatXu: "Việt Nam");

        var skuDen = new Sku("9356381205803",
            [new SkuVariant("Màu sắc", "Đen")],
            [new SkuImage("https://salt.tikicdn.com/ts/product/1f/eb/e3/70ea444c1d20fd520feeb2950e11d95c.png")],
            quantity: 100, isActive: true, price: Money.FromVND((long)(4990000)));

        var variantMau = new ProductVariant("Màu sắc");
        variantMau.AddOption("Đen");
        var variants = new List<ProductVariant> { variantMau };

        const string description = """
            <p><img src="https://salt.tikicdn.com/cache/w750/ts/product/85/83/09/313629e1d40e1193beb45894755504e1.jpg_.webp" alt="" /></p>
            <p><img src="https://salt.tikicdn.com/cache/w750/ts/product/27/ae/91/cbcf6b77740ea91fcd66c61d02fb01e4.jpg_.webp" alt="" /></p>
            <p><img src="https://salt.tikicdn.com/cache/w750/ts/product/3f/e1/ac/e70a777eb9dae84ba6cc61fff31f4e5b.jpg_.webp" alt="" /></p>
            <p><img src="https://salt.tikicdn.com/cache/w750/ts/product/8f/3a/e9/a209a664b863b9011130b81f99b2b271.jpg_.webp" alt="" /></p>
            <p><img src="https://salt.tikicdn.com/cache/w750/ts/product/72/33/8f/9fdf4f7d37872f40b6b6bde8f3bfb6a5.jpg_.webp" alt="" /></p>
            """;

        return BuildFull("Điện Thoại Samsung Galaxy A16 5G (4GB/128GB) - Đã Kích Hoạt Bảo Hành Điện Tử - Hàng Chính Hãng",
            "dien-thoai-samsung-galaxy-a16-5g-4gb-128gb-da-kich-hoat-bao-hanh-dien-tu-hang-chinh-hang",
            "Samsung Galaxy A16 5G, 4/128GB, bảo hành chính hãng.",
            images, attributes, [skuDen], variants, description, now, categoryId);
    }

    // ── Product 7: Apple iPhone 17e ───────────────────────────────────────────────
    private static Product BuildIPhone17e(DateTimeOffset now, Dictionary<string, int> attrMap, int categoryId)
    {
        var images = new List<ProductImage>
        {
            new(0, "https://salt.tikicdn.com/ts/product/7b/51/47/5c76f01762ad765dec469005ab96833a.jpg"),
        };

        var attributes = BuildMobileAttrs(attrMap, thuongHieu: "Apple", xuatXu: "China");

        var variantMau = new ProductVariant("Màu");
        variantMau.AddOption("Trắng"); variantMau.AddOption("Đen"); variantMau.AddOption("Hồng");
        var variantDungLuong = new ProductVariant("Dung lượng");
        variantDungLuong.AddOption("256GB");
        var variants = new List<ProductVariant> { variantMau, variantDungLuong };

        var skuTrang = new Sku("8390602435727",
            [new SkuVariant("Màu", "Trắng"), new SkuVariant("Dung lượng", "256GB")],
            [new SkuImage("https://salt.tikicdn.com/ts/product/7b/51/47/5c76f01762ad765dec469005ab96833a.jpg")],
            quantity: 100, isActive: true, price: Money.FromVND((long)(16990000)));
        var skuDen = new Sku("9989498460265",
            [new SkuVariant("Màu", "Đen"), new SkuVariant("Dung lượng", "256GB")],
            [new SkuImage("https://salt.tikicdn.com/ts/product/c1/99/ce/7ee3dad9756ea2122ffe5040c66f73dc.jpg")],
            quantity: 100, isActive: true, price: Money.FromVND((long)(16990000)));
        var skuHong = new Sku("6612557182714",
            [new SkuVariant("Màu", "Hồng"), new SkuVariant("Dung lượng", "256GB")],
            [new SkuImage("https://salt.tikicdn.com/ts/product/fb/16/51/9a75ea240dd4dc37a30fef4e877cdac0.jpg")],
            quantity: 100, isActive: true, price: Money.FromVND((long)(16990000)));

        const string description = """
            <p><strong>iPhone 17e mang đến trải nghiệm cao cấp bên trong thiết kế gọn gàng với màn hình Super Retina XDR 6.1 inch.</strong></p>
            <h2>Tổng quan những điểm nhấn nổi bật của iPhone 17e</h2>
            <ul>
            <li>Màn hình 6.1 inch, độ phân giải 2.532 x 1.170 pixels, 460 ppi, sáng đến 1200 nit HDR</li>
            <li>Chip Apple A19 với CPU 6 lõi, tối ưu đa nhiệm và tiết kiệm điện</li>
            <li>Camera 48 MP Fusion kèm Telephoto 2x 12 MP</li>
            <li>Thời lượng pin xem video lên đến 26 giờ</li>
            <li>Bộ nhớ từ 256 GB, có tùy chọn 512 GB</li>
            <li>Chạy iOS 26 tích hợp Apple Intelligence</li>
            </ul>
            <h2>Giới thiệu về iPhone 17e</h2>
            <p>Để tối ưu độ bền, đội ngũ Apple đã hoàn thiện sản phẩm với cấu trúc khung nhôm chắc chắn và phủ kính Ceramic Shield 2 để bảo vệ không gian hiển thị. Kích thước màn hình 6.1 inch giúp thiết bị giữ được sự cân đối giữa không gian hiển thị rộng rãi và khả năng cầm nắm thoải mái. iPhone 17e lên kệ với ba màu Đen, Trắng và Hồng.</p>
            <h2>Màn hình Super Retina XDR sắc nét</h2>
            <p>Không gian hiển thị có độ phân giải 2.532 x 1.170 pixels, mật độ 460 ppi. Dải màu rộng P3, độ sáng tiêu chuẩn 800 nit, HDR 1200 nit.</p>
            <h2>Hiệu năng Apple A19</h2>
            <p>Chip Apple A19 với CPU 6 lõi, gồm 2 lõi hiệu năng và 4 lõi tiết kiệm điện, tối ưu đa nhiệm và AI.</p>
            <h2>Camera 48 MP Fusion</h2>
            <p>Camera chính 48 MP Fusion có thể hoạt động linh hoạt như hai ống kính trong một. Telephoto 2x 12 MP hỗ trợ zoom chất lượng cao. Camera trước 12 MP f/1.9.</p>
            """;

        return BuildFull("Apple iPhone 17e",
            "apple-iphone-17e",
            "iPhone 17e, chip A19, Apple Intelligence, màn hình 6.1 inch, 3 màu.",
            images, attributes, [skuTrang, skuDen, skuHong], variants, description, now, categoryId);
    }

    // ── Product 8: Samsung Galaxy S26 Ultra ──────────────────────────────────────
    private static Product BuildSamsungS26Ultra(DateTimeOffset now, Dictionary<string, int> attrMap, int categoryId)
    {
        var images = new List<ProductImage>
        {
            new(0, "https://salt.tikicdn.com/cache/w1200/ts/product/43/51/c7/591084ec0b8bbac62ec5458dadf251f8.jpg"),
            new(0, "https://salt.tikicdn.com/cache/w1200/ts/product/15/4e/f7/4e9f6726bdaee1d7580d728638d294f9.jpg"),
            new(0, "https://salt.tikicdn.com/cache/w1200/ts/product/dd/1f/62/0a107627be4ef7d21996c714dfc54365.jpg"),
            new(0, "https://salt.tikicdn.com/cache/w1200/ts/product/6e/ce/49/44d436929ac2fd2840e1bc4d297df984.jpg"),
            new(0, "https://salt.tikicdn.com/cache/w1200/ts/product/4e/95/c8/123b41eed716b3119141b82311166a2f.jpg"),
            new(0, "https://salt.tikicdn.com/cache/w1200/ts/product/64/a9/6e/98db3a9de543761d556262283fdd83cb.jpg"),
            new(0, "https://salt.tikicdn.com/cache/w1200/ts/product/6e/cc/0c/f32f233eba437c4388b278aa29f59e65.jpg"),
            new(0, "https://salt.tikicdn.com/cache/w1200/ts/product/d1/0d/72/368aa14e78c996e04be5cc25ad0123f6.jpg"),
        };

        var attributes = BuildMobileAttrs(attrMap,
            thuongHieu: "Samsung", xuatXu: "Việt Nam", dungLuongPin: "5000 mAh",
            manHinh: "Dynamic AMOLED 2X", cameraSau: "200 MP + 50 MP + 10 MP + 12 MP", cameraTruoc: "12 MP",
            cpu: "Snapdragon 8 Elite");

        var variantMau = new ProductVariant("Màu sắc");
        variantMau.AddOption("Trắng"); variantMau.AddOption("Đen"); variantMau.AddOption("Tím");
        var variants = new List<ProductVariant> { variantMau };

        var skuTrang = new Sku("5133113695023",
            [new SkuVariant("Màu sắc", "Trắng")],
            [new SkuImage("https://salt.tikicdn.com/cache/280x280/ts/product/43/51/c7/591084ec0b8bbac62ec5458dadf251f8.jpg")],
            quantity: 100, isActive: true, price: Money.FromVND((long)(41990000)));
        var skuDen = new Sku("3264596578052",
            [new SkuVariant("Màu sắc", "Đen")],
            [new SkuImage("https://salt.tikicdn.com/cache/280x280/ts/product/dd/1f/62/0a107627be4ef7d21996c714dfc54365.jpg")],
            quantity: 100, isActive: true, price: Money.FromVND((long)(41990000)));
        var skuTim = new Sku("6001399413095",
            [new SkuVariant("Màu sắc", "Tím")],
            [new SkuImage("https://salt.tikicdn.com/cache/280x280/ts/product/15/4e/f7/4e9f6726bdaee1d7580d728638d294f9.jpg")],
            quantity: 100, isActive: true, price: Money.FromVND((long)(41990000)));

        const string description = """
            <p><strong>Samsung Galaxy S26 Ultra</strong></p>
            <p>Thiết kế titan sang trọng với màn hình Dynamic AMOLED 2X 6.9 inch cùng bút S Pen tích hợp. Camera 200MP hỗ trợ Zoom quang học 5x và AI chụp ảnh đỉnh cao.</p>
            <p>Chip Snapdragon 8 Elite mạnh mẽ, pin 5000 mAh với sạc nhanh 45W.</p>
            <p><img src="https://salt.tikicdn.com/ts/tmp/89/5b/90/8d725188ec1136d52c046f8f3fef48f7.jpg" alt="" width="750" height="375" /></p>
            <p><img src="https://salt.tikicdn.com/ts/tmp/23/3e/0c/9537c141ff06a1ced2236cf93695e4ae.png" alt="" width="750" height="418" /></p>
            <p><img src="https://salt.tikicdn.com/ts/tmp/c3/93/31/67df581733a9c610256d213c90cfa5e2.jpg" alt="" width="750" height="375" /></p>
            """;

        return BuildFull("Điện Thoại Samsung Galaxy S26 Ultra (12GB/256GB) - Hàng Chính Hãng",
            "dien-thoai-samsung-galaxy-s26-ultra-12gb-256gb-hang-chinh-hang",
            "Samsung Galaxy S26 Ultra, 12/256GB, chip Snapdragon 8 Elite, camera AI 200MP, bút S Pen.",
            images, attributes, [skuTrang, skuDen, skuTim], variants, description, now, categoryId);
    }

    // ── Product 9: Samsung Galaxy S26 ────────────────────────────────────────────
    private static Product BuildSamsungS26(DateTimeOffset now, Dictionary<string, int> attrMap, int categoryId)
    {
        var images = new List<ProductImage>
        {
            new(0, "https://salt.tikicdn.com/cache/w1200/ts/product/89/5b/90/fd9dc5b5d3f4d2c4e19f3fb01fc4c4d1.jpg"),
            new(0, "https://salt.tikicdn.com/cache/w1200/ts/product/23/3e/0c/b2a8e9c4f5d1a3b7e8c9d0e1f2a3b4c5.jpg"),
            new(0, "https://salt.tikicdn.com/cache/w1200/ts/product/c3/93/31/a1b2c3d4e5f6a7b8c9d0e1f2a3b4c5d6.jpg"),
            new(0, "https://salt.tikicdn.com/cache/w1200/ts/product/02/70/fe/b2c3d4e5f6a7b8c9d0e1f2a3b4c5d6e7.jpg"),
            new(0, "https://salt.tikicdn.com/cache/w1200/ts/product/63/b2/36/c3d4e5f6a7b8c9d0e1f2a3b4c5d6e7f8.jpg"),
        };

        var attributes = BuildMobileAttrs(attrMap,
            thuongHieu: "Samsung", xuatXu: "Việt Nam", dungLuongPin: "4000 mAh",
            manHinh: "Dynamic AMOLED 2X", cameraSau: "50 MP + 12 MP + 10 MP", cameraTruoc: "12 MP",
            cpu: "Snapdragon 8 Elite");

        var variantMau = new ProductVariant("Màu sắc");
        variantMau.AddOption("Trắng"); variantMau.AddOption("Tím");
        variantMau.AddOption("Xanh"); variantMau.AddOption("Đen");
        var variants = new List<ProductVariant> { variantMau };

        var skuTrang = new Sku("7245819036471",
            [new SkuVariant("Màu sắc", "Trắng")],
            [new SkuImage("https://salt.tikicdn.com/cache/280x280/ts/product/89/5b/90/fd9dc5b5d3f4d2c4e19f3fb01fc4c4d1.jpg")],
            quantity: 100, isActive: true, price: Money.FromVND((long)(25990000)));
        var skuTim = new Sku("6384720195802",
            [new SkuVariant("Màu sắc", "Tím")],
            [new SkuImage("https://salt.tikicdn.com/cache/280x280/ts/product/23/3e/0c/b2a8e9c4f5d1a3b7e8c9d0e1f2a3b4c5.jpg")],
            quantity: 100, isActive: true, price: Money.FromVND((long)(25990000)));
        var skuXanh = new Sku("5803096522639",
            [new SkuVariant("Màu sắc", "Xanh")],
            [new SkuImage("https://salt.tikicdn.com/cache/280x280/ts/product/8e/49/54/a8001b59534674a9e60d1e9aade38879.jpg")],
            quantity: 100, isActive: true, price: Money.FromVND((long)(25990000)));
        var skuDen = new Sku("4417290963754",
            [new SkuVariant("Màu sắc", "Đen")],
            [new SkuImage("https://salt.tikicdn.com/cache/280x280/ts/product/8e/49/54/d2acbd5f1d25e3ec25f30407220cbfc4.jpg")],
            quantity: 100, isActive: true, price: Money.FromVND((long)(25990000)));

        const string description = """
            <p><strong>Samsung Galaxy S26</strong></p>
            <p>Thiết kế tinh tế với khung viền siêu mỏng, nhẹ nhàng nhưng bền bỉ với kính Armor Aluminium 2. Màn hình Dynamic AMOLED 2X 6.2 inch tươi sáng rực rỡ.</p>
            <p>Sức mạnh phần cứng vượt trội với Snapdragon 8 Elite, công nghệ dẫn đầu mọi xu hướng. Tính năng AI giúp bạn phiên dịch trực tiếp, tóm tắt ghi chú chỉ với một chạm.</p>
            <p>Camera chính 50MP thu sáng đỉnh cao, hỗ trợ Super HDR nâng cấp chân thực màu sắc đêm.</p>
            <p><img src="https://salt.tikicdn.com/ts/tmp/89/5b/90/8d725188ec1136d52c046f8f3fef48f7.jpg" alt="" width="750" height="375" /></p>
            <p><img src="https://salt.tikicdn.com/ts/tmp/23/3e/0c/9537c141ff06a1ced2236cf93695e4ae.png" alt="" width="750" height="418" /></p>
            <p><img src="https://salt.tikicdn.com/ts/tmp/c3/93/31/67df581733a9c610256d213c90cfa5e2.jpg" alt="" width="750" height="375" /></p>
            <p><img src="https://salt.tikicdn.com/ts/tmp/02/70/fe/ec15f60aeab4b580cd98ad5be1ad28e3.jpg" alt="" width="750" height="375" /></p>
            <p><img src="https://salt.tikicdn.com/ts/tmp/63/b2/36/1be469155cacb9237e615f18b9d64095.jpg" alt="" width="750" height="375" /></p>
            <p><img src="https://salt.tikicdn.com/ts/tmp/4d/57/b8/06c89e0f9cf87b0293ea298d26466ed0.jpg" alt="" width="750" height="375" /></p>
            <p><img src="https://salt.tikicdn.com/ts/tmp/7c/81/cd/fcffb300f32572b9fef46bf5bb2df263.jpg" alt="" width="750" height="375" /></p>
            """;

        return BuildFull("Điện Thoại Samsung Galaxy S26 (12GB/256GB) - Hàng Chính Hãng",
            "dien-thoai-samsung-galaxy-s26-12gb-256gb-hang-chinh-hang",
            "Samsung Galaxy S26, 12/256GB, chip Snapdragon 8 Elite, camera AI 50MP.",
            images, attributes, [skuTrang, skuTim, skuXanh, skuDen], variants, description, now, categoryId);
    }

    // ── Attribute helper ──────────────────────────────────────────────────────────
    private static List<ProductAttribute> BuildMobileAttrs(
        Dictionary<string, int> attrMap,
        string? thuongHieu = null, string? xuatXu = null, string? dungLuongPin = null,
        string? manHinh = null, string? cameraSau = null, string? cameraTruoc = null,
        string? cpu = null)
    {
        var attributes = new List<ProductAttribute>();
        void Add(string name, string? value)
        {
            if (value is null) return;
            if (!attrMap.TryGetValue(name, out var id))
                throw new InvalidOperationException(
                    $"Attribute '{name}' not found. Ensure AttributeSeeder ran before product seeders.");
            attributes.Add(new ProductAttribute(id, freeTextValue: value));
        }

        Add("Thương hiệu",         thuongHieu);
        Add("Xuất xứ (Made in)",   xuatXu);
        Add("Dung lượng pin",      dungLuongPin);
        Add("Loại màn hình",       manHinh);
        Add("Camera sau",          cameraSau);
        Add("Camera trước",        cameraTruoc);
        Add("Chip xử lý (CPU)",    cpu);
        return attributes;
    }

    // ── Full builder ──────────────────────────────────────────────────────────────
    private static Product BuildFull(
        string name, string slug, string shortDescription,
        List<ProductImage> images, List<ProductAttribute> attributes,
        List<Sku> skus, List<ProductVariant> variants,
        string description, DateTimeOffset now, int categoryId)
    {
        var categories = new List<ProductCategory> { new(categoryId) };

        return Product.CreateProduct(
            name:             name,
            slug:             slug,
            description:      description,
            shortDescription: shortDescription,
            status:           ProductStatus.Available,
            sellerId:         SeedConstants.TikiSellerId,
            condition:        ProductCondition.New,
            featured:         false,
            categories:       categories,
            attributes:       attributes,
            images:           images,
            skus:             skus,
            variants:         variants,
            createdAt:        now,
            createdBy:        "seed"
        );
    }
}

