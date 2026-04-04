using HiveSpace.CatalogService.Domain.Aggregates.ProductAggregate;
using HiveSpace.CatalogService.Domain.Common;
using HiveSpace.CatalogService.Domain.Enums;
using HiveSpace.CatalogService.Infrastructure.Data;
using HiveSpace.Domain.Shared.Enumerations;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace HiveSpace.CatalogService.Infrastructure;

public static class BookstoreSeedData
{
    // Category ID 1 = "Nhà Sách Tiki" (from CategorySeedData)
    private const int BookCategoryId = 1;

    public static async Task SeedAsync(CatalogDbContext context, CancellationToken cancellationToken = default)
    {
        var anyExists = await context.Products
            .Where(p => p.Categories.Any(c => c.CategoryId == BookCategoryId))
            .AnyAsync(cancellationToken);
        if (anyExists)
        {
            Log.Debug("Nhà Sách Tiki products already seeded. Skipping.");
            return;
        }

        // Load attribute ID map (name → id) for leaf attributes
        var attrMap = await context.Attributes
            .Where(a => a.ParentId != null)
            .ToDictionaryAsync(a => a.Name, a => a.Id, cancellationToken);

        var now = DateTimeOffset.UtcNow;

        var products = new List<Product>
        {
            BuildAiTangLuong(now, attrMap),
            BuildSongDoiRungRinh(now, attrMap),
            BuildComboDanOngSaoHoa(now, attrMap),
            BuildComboOshoYeu(now, attrMap),
            Build(
                name: "Combo 2 Cuốn: Tớ Học Lập Trình - Làm Quen Với Python + Clean Code – Mã Sạch Và Con Đường Trở Thành Lập Trình Viên Giỏi",
                slug: "combo-2q-to-hoc-lap-trinh-lam-quen-voi-python-clean-code-ma-sach-va-con-duong-tro-thanh-lap-trinh-vien-gioi",
                description: "Combo lý tưởng cho lập trình viên ở mọi cấp độ: từ học Python từ đầu đến viết mã sạch chuyên nghiệp. Cặp đôi sách giúp bạn xây dựng nền tảng vững chắc và kỹ năng viết code chất lượng.",
                shortDescription: "Combo Python cơ bản và Clean Code cho lập trình viên.",
                skuNo: "8667853140955", price: 412200m,
                thumbnailUrl: "https://salt.tikicdn.com/cache/280x280/ts/product/0c/74/4c/724bc959159f76d156bfcbaf1c176b8e.jpg",
                sellerId: StoreSeedData.GiverSellerId, createdAt: now),
            Build(
                name: "Lá Hoa Trên Đường Về - Sa Môn Thích Pháp Hòa",
                slug: "sach-la-hoa-tren-duong-ve-sa-mon-thich-phap-hoa",
                description: "Tập sách pháp thoại của Sa Môn Thích Pháp Hòa mang đến những lời dạy nhẹ nhàng, sâu lắng về cuộc sống và con đường tu tập. Kèm theo bookmark lá bồ đề và postcard đặc biệt.",
                shortDescription: "Pháp thoại nhẹ nhàng về cuộc sống từ Sa Môn Thích Pháp Hòa.",
                skuNo: "8836034836300", price: 84672m,
                thumbnailUrl: "https://salt.tikicdn.com/cache/280x280/ts/product/15/8d/9a/6cdf5d8b69e75654e892dbdb3a94167b.jpg",
                sellerId: StoreSeedData.GiverSellerId, createdAt: now),
            Build(
                name: "Thám Tử Lừng Danh Conan - Tập 107",
                slug: "tham-tu-lung-danh-conan-tap-107",
                description: "Tập 107 của bộ truyện tranh kinh điển Thám Tử Lừng Danh Conan của tác giả Gosho Aoyama. Hành trình phá án ly kỳ tiếp tục với những vụ án mới đầy bất ngờ.",
                shortDescription: "Truyện tranh Conan tập 107 — vụ án mới chờ bạn khám phá.",
                skuNo: "8740385161745", price: 20000m,
                thumbnailUrl: "https://salt.tikicdn.com/cache/280x280/ts/product/5b/8c/d8/760d6721d631b232b540673126d238a2.jpg",
                sellerId: StoreSeedData.PhuongDongSellerId, createdAt: now),
            Build(
                name: "Sách Tô Màu Phát Triển Trí Não Cho Bé 1-5 Tuổi",
                slug: "sach-to-mau-phat-trien-tri-nao-cho-be-1-5-tuoi",
                description: "Bộ sách tô màu được thiết kế khoa học giúp kích thích trí não, phát triển tư duy sáng tạo và khả năng cầm bút cho trẻ từ 1 đến 5 tuổi. Nội dung phong phú, hình ảnh ngộ nghĩnh, màu sắc tươi sáng.",
                shortDescription: "Sách tô màu kích thích phát triển trí não cho bé 1-5 tuổi.",
                skuNo: "1389576092296", price: 8777m,
                thumbnailUrl: "https://salt.tikicdn.com/cache/280x280/ts/product/c2/2c/60/cf3cc397d6db05e5b41677e0db309d91.jpg",
                sellerId: StoreSeedData.PhuongDongSellerId, createdAt: now),
            Build(
                name: "Monster - Deluxe Edition",
                slug: "monster-deluxe-edition",
                description: "Phiên bản deluxe của bộ manga kinh dị tâm lý nổi tiếng Monster của tác giả Naoki Urasawa. Câu chuyện về bác sĩ Tenma và hành trình truy tìm một quái vật mang hình người — kiệt tác không thể bỏ lỡ.",
                shortDescription: "Manga kinh dị tâm lý Monster - phiên bản deluxe cao cấp.",
                skuNo: "8273928944021", price: 95000m,
                thumbnailUrl: "https://salt.tikicdn.com/cache/280x280/ts/product/84/9b/c7/2f34a84035816c04f622084184aef72d.jpg",
                sellerId: StoreSeedData.PhuongDongSellerId, createdAt: now),
            Build(
                name: "Sách Thương Tiến Tửu",
                slug: "sach-thuong-tien-tuu",
                description: "Tiểu thuyết đam mỹ cổ trang Thương Tiến Tửu của tác giả Đường Tửu Khanh — câu chuyện tình cảm lãng mạn đan xen với âm mưu triều đình, được độc giả yêu thích đặc biệt.",
                shortDescription: "Tiểu thuyết đam mỹ cổ trang nổi tiếng của Đường Tửu Khanh.",
                skuNo: "8454905043264", price: 89280m,
                thumbnailUrl: "https://salt.tikicdn.com/cache/280x280/ts/product/60/62/16/014749191af6bc3c23b2813369c96157.png",
                sellerId: StoreSeedData.GiverSellerId, createdAt: now),
        };

        await context.Products.AddRangeAsync(products, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        Log.Debug("Seeded {Count} products for Nhà Sách Tiki.", products.Count);
    }

    // ── Product 1: Ai Tăng Lương Cho Bạn ─────────────────────────────────────
    private static Product BuildAiTangLuong(DateTimeOffset now, Dictionary<string, int> attrMap)
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
            sellerId:         StoreSeedData.GiverSellerId,
            images:           images,
            attributes:       attributes,
            description:      description,
            now:              now);
    }

    // ── Product 2: Sống Đời Rủng Rỉnh Thong Dong ─────────────────────────────
    private static Product BuildSongDoiRungRinh(DateTimeOffset now, Dictionary<string, int> attrMap)
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
            sellerId:         StoreSeedData.GiverSellerId,
            images:           images,
            attributes:       attributes,
            description:      description,
            now:              now);
    }

    // ── Product 3: Combo Đàn Ông Sao Hỏa + Lấy Tình Thâm ────────────────────
    private static Product BuildComboDanOngSaoHoa(DateTimeOffset now, Dictionary<string, int> attrMap)
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
            sellerId:         StoreSeedData.PhuongDongSellerId,
            images:           images,
            attributes:       attributes,
            description:      description,
            now:              now);
    }

    // ── Product 4: Combo 3 cuốn OSHO Yêu + Đàn Ông + Phụ Nữ ─────────────────
    private static Product BuildComboOshoYeu(DateTimeOffset now, Dictionary<string, int> attrMap)
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
            sellerId:         StoreSeedData.PhuongDongSellerId,
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
            if (attrMap.TryGetValue(name, out var id))
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
        Guid sellerId, List<ProductImage> images, List<ProductAttribute> attributes,
        string description, DateTimeOffset now)
    {
        var categories = new List<ProductCategory> { new(BookCategoryId) };

        var skus = new List<Sku>
        {
            new(skuNo, [], images.Take(1).Select(img => new SkuImage(img.FileId)).ToList(),
                quantity: 100, isActive: true, price: new Money(price, Currency.VND))
        };

        var product = new Product(
            name:        name,
            description: description,
            status:      ProductStatus.Available,
            categories:  categories,
            attributes:  attributes,
            images:      images,
            skus:        skus,
            variants:    [],
            createdAt:   now,
            updatedAt:   null,
            createdBy:   "seed",
            updatedBy:   null
        );

        typeof(Product).GetProperty("Slug")?.SetValue(product, slug);
        typeof(Product).GetProperty("ShortDescription")?.SetValue(product, shortDescription);
        typeof(Product).GetProperty("SellerId")?.SetValue(product, sellerId);
        typeof(Product).GetProperty("Featured")?.SetValue(product, false);
        typeof(Product).GetProperty("Condition")?.SetValue(product, ProductCondition.New);

        return product;
    }

    private static Product Build(
        string name, string slug, string description, string shortDescription,
        string skuNo, decimal price, string thumbnailUrl, Guid sellerId, DateTimeOffset createdAt)
    {
        var categories = new List<ProductCategory> { new(BookCategoryId) };
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
        typeof(Product).GetProperty("SellerId")?.SetValue(product, sellerId);
        typeof(Product).GetProperty("Featured")?.SetValue(product, false);
        typeof(Product).GetProperty("Condition")?.SetValue(product, ProductCondition.New);

        return product;
    }
}
