using HiveSpace.CatalogService.Domain.Aggregates.ProductAggregate;
using HiveSpace.Domain.Shared.ValueObjects;
using HiveSpace.CatalogService.Domain.Enums;
using HiveSpace.CatalogService.Infrastructure.Data;
using HiveSpace.Domain.Shared.Enumerations;
using HiveSpace.Infrastructure.Persistence.Seeding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HiveSpace.CatalogService.Infrastructure.SeedData;

internal sealed class BookstoreSeeder(CatalogDbContext db, ILogger<BookstoreSeeder> logger) : ISeeder
{
    public int Order => 4;
    private const int ProductIdStart = 1001;
    private const int SkuIdStart = 10001;

    public async Task SeedAsync(CancellationToken ct = default)
    {
        var categoryMap = await db.Categories.ToDictionaryAsync(c => c.Name, c => c.Id, ct);
        if (!categoryMap.TryGetValue("Nhà Sách Tiki", out var categoryId))
        {
            logger.LogWarning("Category 'Nhà Sách Tiki' not found. Skipping bookstore seed.");
            return;
        }

        var anyExists = await db.Products
            .Where(p => p.Categories.Any(c => c.CategoryId == categoryId))
            .AnyAsync(ct);
        if (anyExists)
        {
            logger.LogDebug("Nhà Sách Tiki products already seeded. Skipping.");
            return;
        }

        var attrMap = await db.Attributes
            .Where(a => a.ParentId != null)
            .ToDictionaryAsync(a => a.Name, a => a.Id, ct);

        var now = DateTimeOffset.UtcNow;
        var products = new List<Product>
        {
            BuildAiTangLuong(now, attrMap, categoryId),
            BuildSongDoiRungRinh(now, attrMap, categoryId),
            BuildComboDanOngSaoHoa(now, attrMap, categoryId),
            BuildComboOshoYeu(now, attrMap, categoryId),
            BuildComboToHocLapTrinh(now, attrMap, categoryId),
            BuildLaHoaTrenDuongVe(now, attrMap, categoryId),
            BuildConan107(now, attrMap, categoryId),
            BuildSachToMau(now, attrMap, categoryId),
            BuildMonsterDeluxe(now, attrMap, categoryId),
            BuildThuongTienTuu(now, attrMap, categoryId),
        };

        var seededSkus = new List<(int ProductId, Sku Sku)>(products.Count);
        for (var i = 0; i < products.Count; i++)
        {
            var product = products[i];
            var productId = ProductIdStart + i;
            var skuId = SkuIdStart + i;

            db.Entry(product).Property(p => p.Id).CurrentValue = productId;

            var sku = product.Skus.Single();
            db.Entry(sku).Property(s => s.Id).CurrentValue = skuId;
            seededSkus.Add((productId, sku));

            product.UpdateSkus([]);
        }

        var strategy = db.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await using var tx = await db.Database.BeginTransactionAsync(ct);

            await db.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT products ON", ct);
            await db.Products.AddRangeAsync(products, ct);
            await db.SaveChangesAsync(ct);
            await db.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT products OFF", ct);

            foreach (var (productId, sku) in seededSkus)
            {
                db.Skus.Add(sku);
                db.Entry(sku).Property("ProductId").CurrentValue = productId;
            }

            await db.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT skus ON", ct);
            await db.SaveChangesAsync(ct);
            await db.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT skus OFF", ct);

            await tx.CommitAsync(ct);
        });
        logger.LogInformation("Seeded {Count} products for Nhà Sách Tiki.", products.Count);
    }

    // ── Product 1: Ai Tăng Lương Cho Bạn ─────────────────────────────────────
    private static Product BuildAiTangLuong(DateTimeOffset now, Dictionary<string, int> attrMap, int categoryId)
    {
        var images = new List<ProductImage>
        {
            new(0, "https://salt.tikicdn.com/ts/product/9b/b4/9c/5a987bea2919a2142f5db4f77c3fb5e7.png"),
            new(0, "https://salt.tikicdn.com/ts/product/1a/93/81/8eeb330966ef1fc7ba332c20e2ab219d.png"),
            new(0, "https://salt.tikicdn.com/ts/product/30/05/c8/ca5085308f31316b285d21b01a123c8f.png"),
            new(0, "https://salt.tikicdn.com/ts/product/81/4d/97/4198cb21a16cba040ff5cb6df4e8680a.png"),
            new(0, "https://salt.tikicdn.com/ts/product/50/0b/b8/bc88f2edbb913342905fd4dd0cca038a.png"),
            new(0, "https://salt.tikicdn.com/ts/product/cd/b1/ff/592a186a9e5c55d749aa2c22574215f2.png"),
            new(0, "https://salt.tikicdn.com/ts/product/1d/71/a9/35e277d0f48d353c50a127a72dbf19d1.png"),
            new(0, "https://salt.tikicdn.com/ts/product/c7/76/e4/e4367bcc37770142e22507d095b8c991.png"),
        };

        var attributes = BuildBookAttrs(attrMap,
            ctyPhatHanh: "Công ty TNHH Truyền Thông Giver",
            loaiBia:     "Bìa mềm",
            soTrang:     "254",
            nxb:         "Nhà Xuất Bản Thế Giới",
            ngayXb:      null);

        const string description = """
            <h2 style="text-align:justify;"><strong>Bí quyết tăng lương, thăng chức và tìm thấy hạnh phúc trong công việc được gói gọn trong những câu chuyện công sở gần gũi, thú vị.</strong></h2>
            <p style="text-align:justify;"><strong>Quyển sách mang đến cho bạn:</strong></p>
            <ul style="text-align:justify;">
            <li>Bí quyết tăng lương trong thời gian ngắn nhất</li>
            <li>Phương pháp thăng tiến bất kể xuất phát điểm</li>
            <li>Tư duy teamwork hiệu quả để thoát khỏi nỗi ám ảnh mang tên gánh team</li>
            <li>Bí quyết nhảy việc khôn ngoan và chọn được công ty phù hợp</li>
            <li>Cách xác định động lực thật sự để có được niềm vui và hạnh phúc trong công việc</li>
            <li>3 bí quyết thăng tiến trong công việc, 3 bảng tự đánh giá và 3 công cụ giúp bạn tháo gỡ mọi vấn đề</li>
            </ul>
            <p style="text-align:justify;"><strong>VỀ TÁC GIẢ</strong></p>
            <p style="text-align:justify;">Doanh nhân <strong>Nguyễn Quốc Tuấn</strong> là chuyên gia ngành bán lẻ thời trang và bán hàng đa kênh (Omnichannel). Anh được biết đến nhiều nhất ở cương vị nguyên sáng lập viên và CEO của hãng giày thời trang Juno và nguyên CEO của Hoàng Phúc International.</p>
            """;

        return BuildFull(
            name:             "Ai Tăng Lương Cho Bạn? 3 Bí Quyết Đơn Giản Để Thăng Tiến Và Hạnh Phúc Trong Công Việc",
            slug:             "ai-tang-luong-cho-ban-3-bi-quyet-don-gian-de-thang-tien-va-hanh-phuc-trong-cong-viec",
            shortDescription: "3 bí quyết đơn giản để thăng tiến và hạnh phúc trong công việc.",
            skuNo:            "3251356516650",
            price:            149000m,
            sellerId:         SeedConstants.GiverSellerId,
            categoryId:       categoryId,
            images:           images,
            attributes:       attributes,
            description:      description,
            now:              now);
    }

    // ── Product 2: Sống Đời Rủng Rỉnh Thong Dong ─────────────────────────────
    private static Product BuildSongDoiRungRinh(DateTimeOffset now, Dictionary<string, int> attrMap, int categoryId)
    {
        var images = new List<ProductImage>
        {
            new(0, "https://salt.tikicdn.com/ts/product/a5/33/3b/7df8544dc35fe951c9f1e8d7e11af086.png"),
            new(0, "https://salt.tikicdn.com/ts/product/c5/27/f2/74caaff9befc3d584bb4eb0e49d68bff.png"),
            new(0, "https://salt.tikicdn.com/ts/product/2f/99/1f/56519f1f711e17e0db92dc3ac62e4da9.png"),
            new(0, "https://salt.tikicdn.com/ts/product/ea/e2/fb/2bd37c350866427c6b29167da2b41cf6.png"),
            new(0, "https://salt.tikicdn.com/ts/product/5d/eb/d0/5855d8a3707fcee0c9b1f20330dd9d67.png"),
            new(0, "https://salt.tikicdn.com/ts/product/97/02/9d/43d6567159594cc431106586f29d4494.png"),
            new(0, "https://salt.tikicdn.com/ts/product/d0/1f/26/85293d007816538059223f596bd03bea.png"),
            new(0, "https://salt.tikicdn.com/ts/product/08/c8/69/9f2f3e18fc5f9e6bd84d0b299757387a.png"),
            new(0, "https://salt.tikicdn.com/ts/product/89/ff/f7/966e4aa2b274644acf77159252cb0028.png"),
            new(0, "https://salt.tikicdn.com/ts/product/84/46/59/faee8952cc5a386bb735981205da919c.png"),
        };

        var attributes = BuildBookAttrs(attrMap,
            ctyPhatHanh: "Công ty TNHH Truyền Thông Giver",
            loaiBia:     "Bìa mềm",
            soTrang:     "264",
            nxb:         "Nhà Xuất Bản Thế Giới",
            ngayXb:      "2025-08-15");

        const string description = """
            <p>Tài chính cá nhân cần được xuất phát từ "cá nhân" rồi mới đến "tài chính". Bởi vì để ra các quyết định khôn ngoan liên quan đến tiền bạc, bạn cần hiểu mình và hiểu mình muốn gì trong cuộc đời.</p>
            <p><strong>Cuốn sách sẽ cung cấp cho bạn những hiểu biết mới về:</strong></p>
            <ul>
            <li>Sự an tâm tài chính không nằm ở số tiền bạn sở hữu, mà nằm ở một từ khóa: sự sáng rõ.</li>
            <li>Ngôi nhà an tâm tài chính có bốn trụ cột, năm thành phần nền móng.</li>
            <li>Phương pháp cấu trúc nguồn thu nhập giống như quy hoạch một khu vườn.</li>
            <li>Không phải giảm chi, cũng không phải tăng chi, chìa khóa để chi tiêu tốt hơn là rõ chi.</li>
            <li>Đầu tư không bắt đầu từ việc đem tiền mua đất, mua vàng hay chứng khoán.</li>
            </ul>
            <p>Hành trình làm chủ tài chính cá nhân chưa bao giờ là dễ dàng, nhưng khi đã nhìn thấy được con đường, sớm muộn chúng ta cũng sẽ tới đích!</p>
            <p><strong>VỀ TÁC GIẢ</strong></p>
            <p><strong>Lê Hoàng Linh</strong> — Nhà sáng lập FIN4P, chuyên gia tư vấn tài chính cá nhân với hơn 14 năm kinh nghiệm trong ngành tài chính và đầu tư (Grant Thornton, VinaCapital).</p>
            """;

        return BuildFull(
            name:             "Sống Đời Rủng Rỉnh Thong Dong - Quán Xuyến Chuyện Tiền Nong, Hướng Về An Tâm Tài Chính",
            slug:             "sach-song-doi-rung-rinh-thong-dong-quan-xuyen-chuyen-tien-nong-huong-ve-an-tam-tai-chinh",
            shortDescription: "Hướng dẫn quản lý tài chính cá nhân, hướng về an tâm tài chính.",
            skuNo:            "7847043795900",
            price:            143200m,
            sellerId:         SeedConstants.GiverSellerId,
            categoryId:       categoryId,
            images:           images,
            attributes:       attributes,
            description:      description,
            now:              now);
    }

    // ── Product 3: Combo Đàn Ông Sao Hỏa + Lấy Tình Thâm ────────────────────
    private static Product BuildComboDanOngSaoHoa(DateTimeOffset now, Dictionary<string, int> attrMap, int categoryId)
    {
        var images = new List<ProductImage>
        {
            new(0, "https://salt.tikicdn.com/ts/product/ea/f7/82/a783af353ebf7dd91319b414b266c3b8.jpg"),
            new(0, "https://salt.tikicdn.com/ts/product/e8/77/c6/3b2e45875c2c70391badf1b5bfc7bfd0.jpg"),
        };

        var attributes = BuildBookAttrs(attrMap,
            ctyPhatHanh: "Nhiều công ty phát hành",
            loaiBia:     "Bìa mềm",
            soTrang:     null,
            nxb:         "Nhiều Nhà Xuất Bản",
            ngayXb:      "2021-06-26");

        const string description = """
            <p><strong>Combo 2 Cuốn Sách Kinh Điển Dạy Bạn Cách Hiểu Bạn Đời:</strong></p>
            <p><strong>Đàn Ông Sao Hỏa Đàn Bà Sao Kim</strong></p>
            <p>Ngày xửa ngày xưa, những người sao Hỏa và sao Kim đã gặp gỡ, yêu nhau và sống hạnh phúc bởi họ tôn trọng và chấp nhận mọi điều khác biệt. Rồi họ đến Trái Đất và chứng lãng quên đã xảy ra: Họ quên rằng họ đến từ những hành tinh khác.</p>
            <p>Cuốn sách HAY NHẤT MỌI THỜI ĐẠI về thấu hiểu người khác giới và tạo hạnh phúc trong hôn nhân, gia đình.</p>
            <p><strong>Lấy Tình Thâm Mà Đổi Đầu Bạc</strong></p>
            <p>Vãn Tình vốn được biết đến là tác giả bộ ba best-seller. Sự trở lại lần này với Lấy tình thâm mà đổi đầu bạc hứa hẹn mang đến cho độc giả những câu chuyện tình yêu, cuộc sống, hôn nhân, gia đình đầy gần gũi và ấm áp.</p>
            <p>Cuốn sách như một liều thuốc giúp xoa dịu những tâm hồn đang đau khổ trong tình yêu, tiếp thêm niềm tin và hi vọng cho những người đang theo đuổi một hôn nhân hạnh phúc.</p>
            """;

        return BuildFull(
            name:             "Combo 2 Cuốn Sách: Đàn Ông Sao Hỏa Đàn Bà Sao Kim + Lấy Tình Thâm Mà Đổi Đầu Bạc (Đọc Để Có Cuộc Sống Hạnh Phúc)",
            slug:             "combo-2-cuon-sach-dan-ong-sao-hoa-dan-ba-sao-kim-lay-tinh-tham-ma-doi-dau-bac",
            shortDescription: "Combo 2 cuốn kinh điển về hôn nhân và tình yêu.",
            skuNo:            "5143054723374",
            price:            250500m,
            sellerId:         SeedConstants.PhuongDongSellerId,
            categoryId:       categoryId,
            images:           images,
            attributes:       attributes,
            description:      description,
            now:              now);
    }

    // ── Product 4: Combo 3 cuốn OSHO Yêu + Đàn Ông + Phụ Nữ ─────────────────
    private static Product BuildComboOshoYeu(DateTimeOffset now, Dictionary<string, int> attrMap, int categoryId)
    {
        var images = new List<ProductImage>
        {
            new(0, "https://salt.tikicdn.com/ts/product/5c/2c/6e/b132dd51b7db43b8a9b2694d0b7b609f.jpg"),
        };

        var attributes = BuildBookAttrs(attrMap,
            ctyPhatHanh: "Nhiều công ty phát hành",
            loaiBia:     "Bìa mềm",
            soTrang:     null,
            nxb:         "Nhiều Nhà Xuất Bản",
            ngayXb:      "2021-03-26");

        const string description = """
            <p><span style="color: #ff0000;"><strong>1. OSHO - Yêu - Being In Love</strong></span></p>
            <p>Một chỉ dẫn "yêu không sợ hãi" đầy ngạc nhiên từ bậc thầy tâm linh Osho. Trong cuốn sách Yêu, Osho đưa ra những kiến giải sâu sắc về nhu cầu tâm lý có sức mạnh lớn nhất của nhân loại và chỉ cho chúng ta cách trải nghiệm tình yêu.</p>
            <p><span style="color: #ff0000;"><strong>2. Osho Đàn Ông - The Book Of Men</strong></span></p>
            <p>Trong cuốn Đàn ông, tác giả phân tích các vai trò khác nhau của người đàn ông, cũng như việc họ đã định hình và ảnh hưởng lên xã hội như thế nào. Đồng thời, ông chỉ ra làm thế nào đàn ông có thể chuyển hướng nguồn năng lượng sang sáng tạo và tiến hoá.</p>
            <p><span style="color: #ff0000;"><strong>3. Osho Phụ Nữ - The Book Of Women</strong></span></p>
            <p>Trong cuốn Phụ nữ, tác giả phân tích giá trị và tầm quan trọng của sức mạnh nữ tính. Ông nghiên cứu các niềm tin, định kiến bị áp đặt lên phụ nữ, từ đó giải phóng họ và khẳng định lại những phẩm chất tuyệt vời của người phụ nữ.</p>
            <p>Osho được tờ Sunday Times của London mô tả là một trong 1000 người kiến tạo của thế kỷ 20.</p>
            """;

        return BuildFull(
            name:             "Combo 3 cuốn: OSHO - Yêu - Being In Love + Osho Đàn Ông - The Book Of Men + Osho Phụ Nữ - The Book Of Women",
            slug:             "combo-3-cuon-osho-yeu-being-in-love-osho-dan-ong-the-book-of-men-osho-phu-nu-the-book-of-women",
            shortDescription: "Trọn bộ 3 cuốn Osho về tình yêu, đàn ông và phụ nữ.",
            skuNo:            "2352229843300",
            price:            302700m,
            sellerId:         SeedConstants.PhuongDongSellerId,
            categoryId:       categoryId,
            images:           images,
            attributes:       attributes,
            description:      description,
            now:              now);
    }

    // ── Product 5: Combo 2 Cuốn Tớ Học Lập Trình ──────────────────────────────
    private static Product BuildComboToHocLapTrinh(DateTimeOffset now, Dictionary<string, int> attrMap, int categoryId)
    {
        var images = new List<ProductImage>
        {
            new(0, "https://salt.tikicdn.com/ts/product/52/ed/fb/484f032ea38a92bb080d8211ae55a039.jpg"),
            new(0, "https://salt.tikicdn.com/ts/product/1b/fa/c1/c3115ee82bcc5001db442a0ad8581332.jpg"),
            new(0, "https://salt.tikicdn.com/ts/product/53/91/b2/7bd4b0d9ce68a5d934dde4d657751680.jpg"),
        };

        var attributes = BuildBookAttrs(attrMap,
            ctyPhatHanh: "Nhã Nam",
            loaiBia:     "Bìa mềm",
            soTrang:     "96",
            nxb:         "Nhà Xuất Bản Thế Giới",
            ngayXb:      "2021-12-12");

        const string description = """
            <h3><strong>Combo 2 Cuốn Tớ Học Lập Trình: Tớ Học Lập Trình - Làm Quen Với Python + Tớ Học Lập Trình - Làm Quen Với Lập Trình Scratch</strong></h3>
            <h3><strong>Lời nói đầu/Giới thiệu sách</strong></h3>
            <ul>
            <li><strong>Tớ học lập trình - Làm quen với Python:</strong></li>
            </ul>
            <ul>
            <li>Sách hướng dẫn lập trình cho các bạn mới học cách dùng ngôn ngữ máy tính Python.</li>
            <li>Chỉ dẫn từng bước để bạn biết cách lập trình, tạo trò chơi, vẽ và làm đủ trò hay ho với Python.</li>
            <li>Chú giải các thuật ngữ máy tính đầy đủ và rõ ràng ở cuối sách.</li>
            <li>Thật nhiều trang web hữu ích để bạn tìm hiểu thêm trên mạng và tải về các mã lệnh cần thiết.</li>
            </ul>
            <ul>
            <li><strong>Tớ học lập trình - Làm quen với lập trình Scratch:</strong></li>
            </ul>
            <ul>
            <li>Cẩm nang hướng dẫn hoàn chỉnh và đơn giản nhất dành cho bạn trẻ bắt đầu học lập trình.</li>
            <li>Ngôn ngữ lập trình Scratch đặc biệt phù hợp cho bạn trẻ mới học, vì tính tương tác trực quan, đồ họa sống động.</li>
            <li>Chỉ cần nắm và kéo các khối lệnh đầy màu sắc có sẵn để lắp ghép thành một kịch bản điều khiển các đối tượng trên màn hình.</li>
            </ul>
            <h3><strong>Đối tượng sử dụng</strong></h3>
            <ul><li>Dành cho bé từ 6 tuổi trở lên</li></ul>
            <h3><strong>Ưu điểm</strong></h3>
            <ul>
            <li><strong>Dễ hiểu:</strong> Ngôn ngữ sử dụng đơn giản, dễ hiểu, phù hợp với cả người mới bắt đầu.</li>
            <li><strong>Học bằng cách làm:</strong> Bạn sẽ được thực hành ngay từ những bài tập đầu tiên.</li>
            <li><strong>Minh họa sinh động:</strong> Các hình ảnh minh họa sinh động giúp bạn dễ dàng hình dung các khái niệm.</li>
            <li><strong>Nội dung phong phú:</strong> Cuốn sách bao gồm nhiều chủ đề khác nhau, giúp bạn có cái nhìn tổng quan về Python và Scratch.</li>
            </ul>
            """;

        return BuildFull(
            name:             "Combo 2 Cuốn Tớ Học Lập Trình: Tớ Học Lập Trình - Làm Quen Với Python + Tớ Học Lập Trình - Làm Quen Với Lập Trình Scratch",
            slug:             "combo-2-cuon-to-hoc-lap-trinh-lam-quen-voi-python-lam-quen-voi-lap-trinh-scratch",
            shortDescription: "Combo lập trình cho bé: Làm quen Python và Scratch qua từng bước thực hành.",
            skuNo:            "8070440105711",
            price:            156000m,
            sellerId:         SeedConstants.GiverSellerId,
            categoryId:       categoryId,
            images:           images,
            attributes:       attributes,
            description:      description,
            now:              now);
    }

    // ── Product 6: Lá Hoa Trên Đường Về ───────────────────────────────────────
    private static Product BuildLaHoaTrenDuongVe(DateTimeOffset now, Dictionary<string, int> attrMap, int categoryId)
    {
        var images = new List<ProductImage>
        {
            new(0, "https://salt.tikicdn.com/ts/product/15/8d/9a/6cdf5d8b69e75654e892dbdb3a94167b.jpg"),
            new(0, "https://salt.tikicdn.com/ts/product/16/0e/a1/c0e86ae79a376559959d756d577f2aab.jpeg"),
            new(0, "https://salt.tikicdn.com/ts/product/98/44/13/b833174bbdd377a0eb76422a675c8841.jpg"),
            new(0, "https://salt.tikicdn.com/ts/product/a1/34/94/0a052acd1d637b396f6f47a0542d445d.jpg"),
            new(0, "https://salt.tikicdn.com/ts/product/f0/92/c9/1fcf6fa6810e3ce1be4ce013cf07be9c.jpg"),
            new(0, "https://salt.tikicdn.com/ts/product/db/2d/84/95a96c8bb158ff22824e3463f936cec8.jpg"),
            new(0, "https://salt.tikicdn.com/ts/product/af/85/0f/7ada1013dbb9bf80463983513b4b0ccb.jpg"),
            new(0, "https://salt.tikicdn.com/ts/product/aa/6a/50/229b5fd0fbd50099ff53b2d0e294bf51.jpg"),
            new(0, "https://salt.tikicdn.com/ts/product/8f/dc/d8/6e2dbeea971cffd73df6d8aeb0bffb7a.jpg"),
        };

        var attributes = BuildBookAttrs(attrMap,
            ctyPhatHanh: "First News - Trí Việt",
            loaiBia:     "Bìa mềm",
            soTrang:     "296",
            nxb:         "Nhà Xuất Bản Dân Trí",
            ngayXb:      null);

        const string description = """
            <p>Trong quá trình thực hiện hai quyển sách <em>Chia sẻ từ trái tim</em> và <em>Con đường chuyển hóa</em> của thầy Thích Pháp Hòa, ban biên tập nhận ra rằng có những thắc mắc của đại chúng xoay quanh các vấn đề rất thiết thực trong đời sống mà phạm vi các bài pháp thoại khó đáp ứng được. Do đó, chúng tôi đã quyết định thực hiện quyển sách thứ ba như một tuyển tập các câu vấn đáp giữa thầy Pháp Hòa với đại chúng trong các buổi giảng pháp của thầy suốt mấy mươi năm qua.</p>
            <p>Khác với những chủ đề thầy Pháp Hòa chủ động chọn để giảng, các câu hỏi từ phía đại chúng lại mở ra nhiều hướng đề tài mới, cho thấy nhiều mối quan tâm, trăn trở có thật trong cuộc sống thường nhật dường như không tìm thấy lời giải đáp trong kinh sách.</p>
            <p>Mặc dù nội dung câu hỏi vô cùng đa dạng và lắm khi "khó đỡ", nhưng với sự thông hiểu Phật pháp và khả năng diễn đạt giản dị, dễ hiểu, sự tận tâm và lòng từ bi, thầy Thích Pháp Hòa đã có thể giải đáp các câu hỏi một cách thỏa đáng.</p>
            <p>Chúng tôi hy vọng quyển sách <strong>Lá hoa trên đường về</strong> của thầy Thích Pháp Hòa sẽ đóng góp thêm một mảnh ghép giúp quý độc giả tháo gỡ được những gút mắc, khai thông được những chỗ bế tắc trong tu tập và trong đời sống, có được cái nhìn dung thông về mọi sự, từ đó có thể an tâm, vô ngại trong mỗi phút giây của cuộc đời.</p>
            """;

        return BuildFull(
            name:             "Sách - Lá Hoa Trên Đường Về - Sa Môn Thích Pháp Hoà - Tặng Kèm Bookmark Lá Bồ Đề Random 1 Trong 3 Mẫu + Bookmark + Postcard",
            slug:             "sach-la-hoa-tren-duong-ve-sa-mon-thich-phap-hoa",
            shortDescription: "Tuyển tập vấn đáp Phật pháp nhẹ nhàng, sâu lắng từ thầy Thích Pháp Hòa.",
            skuNo:            "8836034836300",
            price:            105840m,
            sellerId:         SeedConstants.GiverSellerId,
            categoryId:       categoryId,
            images:           images,
            attributes:       attributes,
            description:      description,
            now:              now);
    }

    // ── Product 7: Thám Tử Lừng Danh Conan - Tập 107 ─────────────────────────
    private static Product BuildConan107(DateTimeOffset now, Dictionary<string, int> attrMap, int categoryId)
    {
        var images = new List<ProductImage>
        {
            new(0, "https://salt.tikicdn.com/ts/product/5b/8c/d8/760d6721d631b232b540673126d238a2.jpg"),
            new(0, "https://salt.tikicdn.com/ts/product/84/6d/36/9171810107aa035465e16868e8313ab1.jpg"),
        };

        var attributes = BuildBookAttrs(attrMap,
            ctyPhatHanh: "Nhà Xuất Bản Kim Đồng",
            loaiBia:     "Bìa mềm",
            soTrang:     "176",
            nxb:         "Nhà Xuất Bản Kim Đồng",
            ngayXb:      null);

        const string description = """
            <p>Ngày nhỏ, Ran và Shinichi từng làm hỏng vòi nước trong công viên. Khi đó, 5 học viên của Học viện Cảnh sát đã xuất hiện và giúp họ!</p>
            <p>Lần này, đội thám tử nhí có cơ hội tham quan Trụ sở Cảnh sát tỉnh Nagano!? Nhưng rồi cả bọn bị cuốn vào những sự cố hỏa hoạn đáng ngờ của nhóm người sáng tạo nội dung số…</p>
            <p>Ở một diễn biến khác, ông Kogoro bị nhóm Eri bám đuôi!</p>
            <p>Rốt cuộc, người mà ông bí mật gặp là ai?</p>
            """;

        return BuildFull(
            name:             "Thám Tử Lừng Danh Conan - Tập 107",
            slug:             "tham-tu-lung-danh-conan-tap-107",
            shortDescription: "Conan tập 107 — Ran, Shinichi và những vụ án hỏa hoạn bí ẩn tại Nagano.",
            skuNo:            "8740385161745",
            price:            25000m,
            sellerId:         SeedConstants.PhuongDongSellerId,
            categoryId:       categoryId,
            images:           images,
            attributes:       attributes,
            description:      description,
            now:              now);
    }

    // ── Product 8: Sách Tô Màu Phát Triển Trí Não Cho Bé ─────────────────────
    private static Product BuildSachToMau(DateTimeOffset now, Dictionary<string, int> attrMap, int categoryId)
    {
        var images = new List<ProductImage>
        {
            new(0, "https://salt.tikicdn.com/ts/product/11/d7/be/894c2ecc2d617695ad920f21e3abcae7.jpg"),
            new(0, "https://salt.tikicdn.com/ts/product/ba/94/eb/c1f51038435413d8f49728f18cd78447.jpg"),
            new(0, "https://salt.tikicdn.com/ts/product/48/45/3d/dea2310efbc687e636ef04b594a268b1.jpg"),
            new(0, "https://salt.tikicdn.com/ts/product/fd/38/72/41cdfa19b077fc7e4ad56c66025e553b.jpg"),
        };

        var attributes = BuildBookAttrs(attrMap,
            ctyPhatHanh: "Nhã Nam",
            loaiBia:     "Bìa mềm",
            soTrang:     "10",
            nxb:         "Nhà Xuất Bản Dân Trí",
            ngayXb:      "2021-11-11");

        const string description = """
            <h3>Tô Màu Phát Triển Não Bộ Cho Bé 1-5 Tuổi - Tập 1</h3>
            <ul>
            <li>Bố mẹ biết chứ, trẻ dưới 18 tháng đã có khả năng chuyển tải suy nghĩ và cảm xúc bằng hình ảnh có màu sắc.</li>
            <li>Tô màu giúp trẻ rèn kỹ năng điều khiển vận động của đôi tay, tăng khả năng quan sát, được sáng tạo và tưởng tượng một cách tự nhiên, được luyện khả năng tập trung và kiên trì, tăng khả năng thưởng thức cái đẹp trong cuộc sống.</li>
            <li>Với trẻ nhỏ tuổi, học là chơi và chơi là học. Tranh tô màu chính là những "bài học" có sức mạnh to lớn với trẻ, bố mẹ ạ!</li>
            <li>Hình ảnh minh họa sinh động, màu sắc tươi sáng.</li>
            <li>Kết hợp hình ảnh và từ vựng song ngữ Việt - Anh.</li>
            <li>Các hình vẽ đơn giản, phù hợp với khả năng nhận biết của trẻ nhỏ.</li>
            </ul>
            <h3>Ưu điểm</h3>
            <ul>
            <li><strong>Giúp bé làm quen với từ vựng:</strong> Sách kết hợp hình ảnh và từ vựng song ngữ, giúp bé vừa tô màu vừa học từ mới.</li>
            <li><strong>Phát triển khả năng nhận biết màu sắc:</strong> Các hình ảnh đa dạng giúp bé phân biệt và ghi nhớ các màu sắc cơ bản.</li>
            <li><strong>Tăng kỹ năng vận động tinh:</strong> Việc tô màu giúp bé rèn luyện khả năng cầm bút, phối hợp tay mắt.</li>
            <li><strong>Phát triển trí tưởng tượng:</strong> Các hình ảnh trong sách mở ra không gian cho bé sáng tạo.</li>
            <li><strong>Giúp bé thư giãn và vui chơi:</strong> Tô màu là một hoạt động thú vị, giúp bé giải tỏa căng thẳng.</li>
            </ul>
            """;

        return BuildFull(
            name:             "Sách Tô Màu Phát Triển Trí Não Bộ Cho Bé 1-5 Tuổi - Tập 1",
            slug:             "sach-to-mau-phat-trien-tri-nao-bo-cho-be-1-5-tuoi-tap-1",
            shortDescription: "Sách tô màu song ngữ Việt-Anh, kích thích phát triển trí não cho bé 1-5 tuổi.",
            skuNo:            "9586614921348",
            price:            12320m,
            sellerId:         SeedConstants.PhuongDongSellerId,
            categoryId:       categoryId,
            images:           images,
            attributes:       attributes,
            description:      description,
            now:              now);
    }

    // ── Product 9: Monster - Deluxe Edition ───────────────────────────────────
    private static Product BuildMonsterDeluxe(DateTimeOffset now, Dictionary<string, int> attrMap, int categoryId)
    {
        var images = new List<ProductImage>
        {
            new(0, "https://salt.tikicdn.com/ts/product/84/9b/c7/2f34a84035816c04f622084184aef72d.jpg"),
            new(0, "https://salt.tikicdn.com/ts/product/be/71/3e/27aa28c1ccc9f9bf7531391ba4d3c074.jpg"),
            new(0, "https://salt.tikicdn.com/ts/product/18/3b/91/3ca36234dcdced1b9ef0217978e216e7.png"),
        };

        var attributes = BuildBookAttrs(attrMap,
            ctyPhatHanh: "Nhà Xuất Bản Kim Đồng",
            loaiBia:     "Bìa mềm",
            soTrang:     null,
            nxb:         "Nhà Xuất Bản Kim Đồng",
            ngayXb:      null);

        const string description = """
            <p>Trong cuộc sống thường nhật, ai cũng có những thời điểm bất an. Ngay cả một vị bác sĩ với tương lai đầy hứa hẹn như Kenzo Tenma cũng không phải ngoại lệ. Nhưng anh đâu thể ngờ rằng việc làm tròn bổn phận y đức, cứu người không phân biệt thay vì bất chấp theo đuổi công danh sự nghiệp của mình lại là nguồn cơn tạo ra một sinh vật đáng sợ. Một câu hỏi được đặt ra: Đâu là công lí, và đâu là tội ác?</p>
            <p>Tại nước Đức sau ngày thống nhất, hàng loạt vụ sát hại các cặp vợ chồng trung niên không con cái liên tiếp xảy ra. Hung thủ là một chàng trai trẻ tên Johan! Tại sao hắn lại nhắm vào họ? Bác sĩ Kenzo Tenma bắt đầu lên đường tìm kiếm em gái song sinh của Johan, người có khả năng nắm giữ manh mối về bí mật mang tên "Quái Vật".</p>
            """;

        return BuildFull(
            name:             "Monster - Deluxe Edition - [Chọn Tập Lẻ]",
            slug:             "monster-deluxe-edition-chon-tap-le",
            shortDescription: "Manga kinh dị tâm lý kiệt tác của Naoki Urasawa — phiên bản deluxe cao cấp.",
            skuNo:            "8273928944021",
            price:            118750m,
            sellerId:         SeedConstants.PhuongDongSellerId,
            categoryId:       categoryId,
            images:           images,
            attributes:       attributes,
            description:      description,
            now:              now);
    }

    // ── Product 10: Thương Tiến Tửu ───────────────────────────────────────────
    private static Product BuildThuongTienTuu(DateTimeOffset now, Dictionary<string, int> attrMap, int categoryId)
    {
        var images = new List<ProductImage>
        {
            new(0, "https://salt.tikicdn.com/ts/product/4a/cc/00/765d85f5afe39b8e8f5c351e7f0ac430.png"),
        };

        var attributes = BuildBookAttrs(attrMap,
            ctyPhatHanh: "Cẩm Phong Books",
            loaiBia:     "Bìa mềm",
            soTrang:     "448",
            nxb:         "Nhà Xuất Bản Hà Nội",
            ngayXb:      "2025-10-06");

        const string description = """
            <p><strong><u>Giới thiệu:</u></strong></p>
            <p>"Vận mệnh đã muốn ta suốt kiếp chôn chân tại chốn này, nhưng số mệnh ấy nào phải con đường ta lựa chọn. Cát vàng chôn vùi huynh đệ ta, ta không muốn tiếp tục thần phục số mệnh hư vô nữa. Thánh chỉ không cứu được sĩ tốt của ta, triều đình không nuôi nổi chiến mã của ta, ta không muốn liều mạng vì những thứ đó nữa. Ta muốn băng qua núi xanh kia, ta muốn đánh một trận, vì chính mình."</p>
            <p><strong><u>Đôi nét tác giả</u></strong></p>
            <p>Đường Tửu Khanh sinh năm 1997, thuộc chòm sao Kim Ngưu. Cô bắt đầu sự nghiệp viết tiểu thuyết vào năm 2015 và hiện đang là một cây bút trẻ rất có tiềm năng của nền tảng văn học mạng Tấn Giang.</p>
            <p><strong>Tác phẩm tiêu biểu:</strong> <em>Thương Tiến Tửu, Gai mềm (tạm dịch), Nam thiền (tạm dịch)…</em></p>
            """;

        return BuildFull(
            name:             "Sách Thương Tiến Tửu",
            slug:             "sach-thuong-tien-tuu",
            shortDescription: "Tiểu thuyết đam mỹ cổ trang nổi tiếng của Đường Tửu Khanh.",
            skuNo:            "9736885668230",
            price:            118800m,
            sellerId:         SeedConstants.GiverSellerId,
            categoryId:       categoryId,
            images:           images,
            attributes:       attributes,
            description:      description,
            now:              now);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static List<ProductAttribute> BuildBookAttrs(
        Dictionary<string, int> attrMap,
        string? ctyPhatHanh, string? loaiBia, string? soTrang, string? nxb, string? ngayXb)
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

        Add("Công ty phát hành", ctyPhatHanh);
        Add("Loại bìa",          loaiBia);
        Add("Số trang",          soTrang);
        Add("Nhà xuất bản",      nxb);
        Add("Ngày xuất bản",     ngayXb);
        return attributes;
    }

    private static Product BuildFull(
        string name, string slug, string shortDescription, string skuNo, decimal price,
        Guid sellerId, int categoryId, List<ProductImage> images, List<ProductAttribute> attributes,
        string description, DateTimeOffset now)
    {
        var categories = new List<ProductCategory> { new(categoryId) };
        var skus = new List<Sku>
        {
            new(skuNo, [], images.Take(1).Select(img => new SkuImage(img.FileId)).ToList(),
                quantity: 100, isActive: true, price: Money.FromVND((long)(price)))
        };

        return Product.CreateProduct(
            name:             name,
            slug:             slug,
            description:      description,
            shortDescription: shortDescription,
            status:           ProductStatus.Available,
            sellerId:         sellerId,
            condition:        ProductCondition.New,
            featured:         false,
            categories:       categories,
            attributes:       attributes,
            images:           images,
            skus:             skus,
            variants:         [],
            createdAt:        now,
            createdBy:        "seed"
        );
    }
}

