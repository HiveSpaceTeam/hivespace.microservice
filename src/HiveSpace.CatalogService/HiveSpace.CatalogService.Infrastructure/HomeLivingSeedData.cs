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

        // Load attribute ID map (name → id) for leaf attributes
        var attrMap = await context.Attributes
            .Where(a => a.ParentId != null)
            .ToDictionaryAsync(a => a.Name, a => a.Id, cancellationToken);

        var now = DateTimeOffset.UtcNow;

        var products = new List<Product>
        {
            BuildCocGiuNhietEL8345(now, attrMap),
            BuildBinhGiuNhietEL8295(now, attrMap),
            BuildChaoChongDinhEL5972(now, attrMap),
            BuildBinhGiuNhietEL8299(now, attrMap),
            BuildBoNoiElmichTrimax(now, attrMap),
            BuildScrubDaddy(now, attrMap),
            // Products 7-10: simple Build()
            Build(
                name: "[NEW] Cốc giữ nhiệt inox 304 Elmich EL8345 dung tích 480ML - Hàng Chính Hãng ",
                slug: "new-coc-giu-nhiet-inox-304-elmich-el8345-dung-tich-480ml-hang-chinh-hang",
                description: "Phiên bản mới của cốc giữ nhiệt Elmich EL8345 dung tích 480ml với thiết kế tinh tế, cập nhật. Làm từ inox 304 cao cấp an toàn thực phẩm, lớp chân không giữ nhiệt/lạnh hiệu quả. Nắp có khóa an toàn, tiện lợi khi di chuyển. Có 3 màu sắc để lựa chọn.",
                shortDescription: "Cốc giữ nhiệt Elmich EL8345 480ml phiên bản mới, inox 304, 3 màu.",
                skuNo: "6400137289630",
                price: 102600m,
                thumbnailUrl: "https://salt.tikicdn.com/cache/280x280/ts/product/4e/a9/b9/ee2f1329eda07bff45b944761ca34ff4.jpg",
                createdAt: now
            ),
            Build(
                name: "Miếng bọt biển lau bụi mọi ngóc ngách Damp Duster - miếng xốp lau chùi đa năng",
                slug: "mieng-bot-bien-lau-bui-moi-ngoc-ngach-damp-duster-mieng-xop-lau-chui-da-nang",
                description: "Miếng bọt biển Damp Duster đa năng, lý tưởng để lau sạch bụi bẩn trên mọi bề mặt — kính, gỗ, inox, nhựa. Chỉ cần làm ẩm nhẹ là có thể lau sạch mà không cần dùng hóa chất. Tiết kiệm, thân thiện môi trường và dễ vệ sinh tái sử dụng. Có 3 màu để lựa chọn.",
                shortDescription: "Miếng bọt biển Damp Duster đa năng, lau sạch không cần hóa chất, 3 màu.",
                skuNo: "1668242013483",
                price: 63200m,
                thumbnailUrl: "https://salt.tikicdn.com/cache/280x280/ts/product/a1/4a/88/ea52142a5f504d93b68b29c2869f4ba9.png",
                createdAt: now
            ),
            Build(
                name: "1 gói Diệt chuột dạng viên Hợp Trí Storm 0.005% gói 20 viên",
                slug: "1-goi-diet-chuot-dang-vien-hop-tri-storm-0-005-goi-20-vien",
                description: "Thuốc diệt chuột Hợp Trí Storm dạng viên, hộp 20 viên. Thành phần hoạt chất Brodifacoum 0.005% thế hệ mới hiệu quả cao, chuột chỉ cần ăn 1 lần là có hiệu quả. An toàn khi sử dụng đúng hướng dẫn, ít ảnh hưởng đến vật nuôi và con người.",
                shortDescription: "Thuốc diệt chuột Storm 20 viên, Brodifacoum 0.005%, hiệu quả sau 1 lần.",
                skuNo: "1831551341333",
                price: 25600m,
                thumbnailUrl: "https://salt.tikicdn.com/cache/280x280/ts/product/03/4f/2d/653c61f3f5c0175d2ee4cb6502f94998.png",
                createdAt: now
            ),
            Build(
                name: "Màng bọc thực phẩm PE Ringo, An toàn, Dùng được trong tủ lạnh, lò vi sóng, có lưỡi cắt dạng trượt",
                slug: "mang-boc-thuc-pham-pe-ringo-an-toan-dung-duoc-trong-tu-lanh-lo-vi-song-co-luoi-cat-dang-truot",
                description: "Màng bọc thực phẩm Ringo làm từ nhựa PE an toàn, không chứa PVC hay chất độc hại. Độ bám dính cao, bọc kín thực phẩm, ngăn mùi và giữ độ tươi ngon lâu hơn. Dùng được trong tủ lạnh và lò vi sóng. Lưỡi cắt dạng trượt tiện lợi, dễ sử dụng hàng ngày. Có 2 size để lựa chọn.",
                shortDescription: "Màng bọc thực phẩm PE Ringo, an toàn, dùng được lò vi sóng, lưỡi cắt trượt.",
                skuNo: "7441106796389",
                price: 13600m,
                thumbnailUrl: "https://salt.tikicdn.com/cache/280x280/ts/product/18/be/5e/e46574c015e92545c691e36e11df2c5e.jpg",
                createdAt: now
            ),
        };

        await context.Products.AddRangeAsync(products, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        Log.Debug("Seeded {Count} products for Nhà Cửa - Đời Sống.", products.Count);
    }

    // ── Product 1: Cốc giữ nhiệt Elmich EL8345 ──────────────────────────────────
    private static Product BuildCocGiuNhietEL8345(DateTimeOffset now, Dictionary<string, int> attrMap)
    {
        var images = new List<ProductImage>
        {
            new(0, "https://salt.tikicdn.com/ts/product/2b/1d/bd/5423d42a062b2355da77809294fb3cb5.png"),
            new(0, "https://salt.tikicdn.com/ts/product/2a/e9/7e/556bf38e373de9a5db1c3d95f3b6fcaa.jpg"),
            new(0, "https://salt.tikicdn.com/ts/product/ba/5c/f4/0e2e8c129ea9062615d30d419e705d3b.jpg"),
            new(0, "https://salt.tikicdn.com/ts/product/2f/06/a2/17728f80098c49134d71a9520c83c194.jpg"),
            new(0, "https://salt.tikicdn.com/ts/product/a6/be/5b/311220cdc7ab51eefb9c150261f27fe0.jpg"),
            new(0, "https://salt.tikicdn.com/ts/product/4f/3d/b0/744b1d68a63be081ad45b711ec796c32.jpg"),
        };

        var attributes = BuildHomeLivingAttrs(attrMap,
            thuongHieu: "Elmich",
            xuatXu:     "Trung Quốc");

        const string description = """
            <p><strong>Chất Liệu Inox 304 – Bền Bỉ và An Toàn:</strong><br />Thân cốc được làm từ inox 304, chất liệu chống ăn mòn và an toàn cho sức khỏe<br /><strong>Vỏ Inox Sơn Tĩnh Điện – Đẳng Cấp và Chống Trầy xước:</strong><br />Với lớp vỏ inox sơn tĩnh điện, sản phẩm không chỉ thêm vẻ đẹp sang trọng mà còn chống trầy xước, giữ cho bề mặt luôn mới mẻ và bóng bẩy theo thời gian.<br /><strong>Lớp Silicon Chống Trơn Trượt – An Toàn và Ổn Định:<br /></strong>Thân cốc được bọc lớp silicon trang trí đẹp mắt, cầm nắm và mang đi dễ dàng<br />Đáy cốc cũng được trang bị lớp silicon chống trơn trượt<br /><strong>Giữ Nhiệt nóng và Lạnh Hiệu Quả:</strong><br />Với khả năng giữ nhiệt từ 5 đến 8 giờ và giữ lạnh đến 18 giờ, cốc giữ nhiệt này là người bạn đồng hành lý tưởng cho cả những ngày nóng nực và lạnh buốt. Bạn có thể thưởng thức đồ uống ưa thích mọi lúc, mọi nơi.<br /><strong>Thiết Kế Nhỏ Gọn và Hiện Đại:</strong><br />Với dung tích 480ml, cốc giữ nhiệt này là sự kết hợp hoàn hảo giữa hiệu năng sử dụng và tính di động. Thiết kế nhỏ gọn và hiện đại, trở thành phụ kiện thời trang bên bạn</p>
            <p><img src="https://salt.tikicdn.com/ts/tmp/83/d1/18/7330e0cb5293494229681df48db0e46b.png" alt="" width="1000" height="1000" /></p>
            <p><img src="https://salt.tikicdn.com/ts/tmp/65/a8/c2/122d4de1927fadad86ae27bcd2ade90c.png" alt="" width="1000" height="1000" /></p>
            <p><img src="https://salt.tikicdn.com/ts/tmp/23/95/23/6ac5b1737102190c133dfd538cf204e7.png" alt="" width="1000" height="1000" /></p>
            <p><img src="https://salt.tikicdn.com/ts/tmp/e5/0a/8b/a8fd37737847c2e34a680909cb9c1a5f.png" alt="" width="1000" height="1000" /></p>
            <p><img src="https://salt.tikicdn.com/ts/tmp/c4/f5/2e/0b6cf4681f14e21e03add641fc26b82d.png" alt="" width="1000" height="1000" /></p>
            """;

        return BuildFull(
            name:             "Cốc giữ nhiệt inox 304 Elmich EL8345 dung tích 480ML - Hàng Chính Hãng",
            slug:             "coc-giu-nhiet-inox-304-elmich-el8345-dung-tich-480ml-hang-chinh-hang",
            shortDescription: "Cốc giữ nhiệt inox 304 Elmich EL8345 480ml, nắp chống tràn, nhiều màu.",
            skuNo:            "9916475726151",
            price:            149000m,
            sellerId:         StoreSeedData.TikiSellerId,
            images:           images,
            attributes:       attributes,
            description:      description,
            now:              now);
    }

    // ── Product 2: Bình giữ nhiệt Elmich EL8295 ─────────────────────────────────
    private static Product BuildBinhGiuNhietEL8295(DateTimeOffset now, Dictionary<string, int> attrMap)
    {
        var images = new List<ProductImage>
        {
            new(0, "https://salt.tikicdn.com/ts/product/b6/fe/b4/165dc71a83cea9aaaf58e002cd446722.jpg"),
            new(0, "https://salt.tikicdn.com/ts/product/0b/47/45/d49cbace2e31882249dd4520191c43a8.jpg"),
            new(0, "https://salt.tikicdn.com/ts/product/13/a7/2d/9a72c03e008f37da655f9b236aa45b2e.jpg"),
            new(0, "https://salt.tikicdn.com/ts/product/61/87/2d/1011e28884941bd59bb5e59bcc1ef1d2.jpg"),
            new(0, "https://salt.tikicdn.com/ts/product/67/4a/54/30bd8529d35a5dbedd0de28372e72900.jpg"),
            new(0, "https://salt.tikicdn.com/ts/product/ed/d5/96/9e3e9ffc817172eed497dae32209ff9f.jpg"),
        };

        var attributes = BuildHomeLivingAttrs(attrMap,
            thuongHieu: "Elmich",
            xuatXu:     "Trung Quốc");

        const string description = """
            <p><img src="https://salt.tikicdn.com/ts/tmp/ca/6c/1d/9352f92d294e3a17808fa9dab4fccdbe.jpg" alt="" width="1000" height="1000" /></p>
            <p><img src="https://salt.tikicdn.com/ts/tmp/13/1f/00/02810613e4c9f6a871ee345ad473fbf2.jpg" alt="" width="1000" height="1000" /></p>
            <p><img src="https://salt.tikicdn.com/ts/tmp/dd/f9/9b/5d93f8e3dd3a062d81ee6819acf979a9.jpg" alt="" width="1000" height="1000" /></p>
            <p><img src="https://salt.tikicdn.com/ts/tmp/81/4a/7a/714d77756b3f24fbb3852a10ddb273a9.jpg" alt="" width="1000" height="1000" /></p>
            <p><img src="https://salt.tikicdn.com/ts/tmp/34/0a/0f/63443fb2ead588fa484942865e59c73f.jpg" alt="" width="1000" height="1000" /></p>
            <p><img src="https://salt.tikicdn.com/ts/tmp/48/8d/7d/32e4059b093fefbad11487d3a754595c.jpg" alt="" width="1000" height="1000" /></p>
            <p><img src="https://salt.tikicdn.com/ts/tmp/99/40/54/f15d7d0391f11c63399ca30406b0d794.jpg" alt="" width="1000" height="1000" /></p>
            <p><img src="https://salt.tikicdn.com/ts/tmp/d2/55/2b/d32466a1589f14cb3b1d18724c30b62b.jpg" alt="" width="1000" height="1000" /></p>
            <p><img src="https://salt.tikicdn.com/ts/tmp/64/49/b1/7d7cba2ba082d04d4f082465a9ad2180.jpg" alt="" width="1000" height="1000" /></p>
            """;

        return BuildFull(
            name:             "Bình giữ nhiệt inox 304 Elmich EL8295 dung tích 500ml",
            slug:             "binh-giu-nhiet-inox-304-elmich-el8295-dung-tich-500ml",
            shortDescription: "Bình giữ nhiệt inox 304 Elmich EL8295 500ml, giữ nhiệt 12 giờ, nhiều màu.",
            skuNo:            "6994556055959",
            price:            147000m,
            sellerId:         StoreSeedData.TikiSellerId,
            images:           images,
            attributes:       attributes,
            description:      description,
            now:              now);
    }

    // ── Product 3: Chảo chống dính Elmich EL5972 ─────────────────────────────────
    private static Product BuildChaoChongDinhEL5972(DateTimeOffset now, Dictionary<string, int> attrMap)
    {
        var images = new List<ProductImage>
        {
            new(0, "https://salt.tikicdn.com/ts/product/4b/00/0a/a8078f70ef255fc82e613468f35bcf8b.jpg"),
            new(0, "https://salt.tikicdn.com/ts/product/a4/e6/a3/21217af4a63afd83ce67905f00587d04.jpg"),
            new(0, "https://salt.tikicdn.com/ts/product/35/1b/3a/53f9bbab49cbe0e0519a8a752daba038.jpg"),
            new(0, "https://salt.tikicdn.com/ts/product/5d/13/de/0d8a472c9ed7955be1d5d1bfaec628dc.jpg"),
            new(0, "https://salt.tikicdn.com/ts/product/5c/f7/84/8a9c22b009437266010896b41cfb164a.jpg"),
            new(0, "https://salt.tikicdn.com/ts/product/5c/85/34/f5808302590712616c854f48ad1753c0.jpg"),
        };

        var attributes = BuildHomeLivingAttrs(attrMap,
            thuongHieu: "Elmich",
            xuatXu:     "Việt Nam");

        const string description = """
            <div class="item">
            <h2>Câu Chuyện Sản Phẩm</h2>
            <div class="bic_wrap__ct">
            <p>Mỗi bữa cơm không chỉ là món ăn mà còn là cách bạn chăm sóc sức khỏe và yêu thương gia đình. Với chảo chống dính Elmich, bạn hoàn toàn yên tâm nhờ chất liệu chống dính cao cấp, an toàn tuyệt đối cho sức khỏe. Thiết kế phong cách Châu Âu tinh tế cùng công nghệ hiện đại, giúp thực phẩm chín đều, giữ trọn vitamin và khoáng chất tự nhiên, mang đến hương vị thơm ngon trong từng món ăn. Nhờ khả năng truyền nhiệt và giữ nhiệt vượt trội, chảo không chỉ tiết kiệm nhiên liệu mà còn giúp bạn nấu nướng nhanh hơn, để tận hưởng trọn vẹn những khoảnh khắc yêu thương bên gia đình.</p>
            </div>
            </div>
            <div class="item">
            <h2>Thông tin sản phẩm</h2>
            <div class="bic_wrap__ct">
            <p>– Kiểu dáng hiện đại, màu sắc trẻ trung, năng động<br />– Làm bằng hợp kim nhôm, bên trong phủ Sơn chống dính an toàn và siêu bền, không giải phóng chất PFOA gây ung thư.<br />– Bên ngoài phủ sơn chống bám bẩn giúp dễ dàng vệ sinh<br />– Tay cầm cách nhiệt tuyệt đối, chịu nhiệt lên đến 180 độ C, thoải mái khi cầm nắm<br />– Sử dụng được trên tất cả các loại bếp: bếp từ, bếp điện, bếp ga,…</p>
            </div>
            </div>
            <div class="item">
            <h2>Thông số kỹ thuật</h2>
            <div class="bic_wrap__ct">
            <div class="table_wrap table-responsive">
            <table class="table table-bordered" border="1" cellspacing="0" cellpadding="0">
            <tbody>
            <tr><td>Size (cm)</td><td>Size 20</td></tr>
            <tr><td>Chất liệu</td><td>Nhôm</td></tr>
            <tr><td>Độ dày thành (mm)</td><td>2.4 mm</td></tr>
            <tr><td>Đáy từ</td><td>Đáy từ thường</td></tr>
            <tr><td>Bên trong</td><td>Sơn chống dính màu smockygold</td></tr>
            <tr><td>Bên ngoài</td><td>Sơn màu xanh Pastel</td></tr>
            <tr><td>Thông số kỹ thuật</td><td>200 x 45 x 2.4</td></tr>
            <tr><td>Kích thước sản phẩm (cả quai) mm</td><td>204,8 x 379 x 83</td></tr>
            <tr><td>Trọng lượng (kg)</td><td>0.535</td></tr>
            <tr><td>Nơi sản xuất</td><td>Elmich</td></tr>
            <tr><td>Quốc gia</td><td>Việt Nam</td></tr>
            <tr><td>Thời gian bảo hành</td><td>24 tháng</td></tr>
            </tbody>
            </table>
            </div>
            </div>
            </div>
            <div class="item">
            <h2>Hướng dẫn sử dụng</h2>
            <div class="bic_wrap__ct">
            <p><strong>1. Trước khi sử dụng chảo lần đầu</strong></p>
            <ul>
            <li>Bước 1: Pha hỗn hợp với tỷ lệ một quả chanh hoặc 20-30ml giấm gạo với 0.5 lít nước.</li>
            <li>Bước 2: Đun sôi hỗn hợp ở lửa nhỏ trong khoảng 2-5 phút.</li>
            <li>Bước 3: Sau khi đun, tắt lửa và để hỗn hợp nguội tự nhiên.</li>
            <li>Bước 4: Rửa sạch lại với nước rửa chén bát và lau khô trước khi sử dụng.</li>
            </ul>
            <p><strong>2. Trong quá trình sử dụng</strong></p>
            <ul>
            <li>Không được đun chảo với dầu hoặc thức ăn ở nhiệt độ cao trong thời gian dài.</li>
            <li>Nêm muối, bột nêm vào thức ăn khi nước đang sôi và khuấy nhanh.</li>
            <li>Không để đồ ăn thừa quá 5 giờ trong chảo.</li>
            <li>Không sử dụng chảo trong lò vi sóng.</li>
            <li>Không đổ nước lạnh vào chảo khi còn đang nóng.</li>
            </ul>
            <p><strong>3. Khi dùng xong</strong></p>
            <ul>
            <li>Dùng dầu rửa chén bát rửa sạch trong và ngoài.</li>
            <li>Không nên dùng các vật dụng cọ nồi bằng kim loại.</li>
            <li>Đối với vết ố vàng chỉ cần dùng một miếng vải mềm tẩm chanh hoặc giấm, chùi sạch theo vòng tròn.</li>
            </ul>
            </div>
            </div>
            """;

        return BuildFull(
            name:             "Chảo chống dính Elmich EL5972",
            slug:             "chao-chong-dinh-elmich-el5972-xanh-mint",
            shortDescription: "Chảo chống dính Elmich EL5972 xanh mint, dùng được bếp từ, 3 màu - 2 size.",
            skuNo:            "9396908958144",
            price:            212000m,
            sellerId:         StoreSeedData.TikiSellerId,
            images:           images,
            attributes:       attributes,
            description:      description,
            now:              now);
    }

    // ── Product 4: Bình giữ nhiệt gia đình Elmich EL8299 ────────────────────────
    private static Product BuildBinhGiuNhietEL8299(DateTimeOffset now, Dictionary<string, int> attrMap)
    {
        var images = new List<ProductImage>
        {
            new(0, "https://salt.tikicdn.com/ts/product/2c/41/54/b7d3d8acdcfa218a1180dfc271272788.png"),
            new(0, "https://salt.tikicdn.com/ts/product/29/0b/20/06b78a1077a349ba5f3860a5aa206f22.png"),
            new(0, "https://salt.tikicdn.com/ts/product/40/fe/70/b900cf836a930eb4525d05ab72f2fd00.png"),
            new(0, "https://salt.tikicdn.com/ts/product/6a/3b/b1/7020f0d87f524e38d2df5c89ee336eb7.png"),
            new(0, "https://salt.tikicdn.com/ts/product/8b/e7/e9/1c163ddfa6dc9bb4072a8c342aaa15bc.png"),
            new(0, "https://salt.tikicdn.com/ts/product/a6/a3/94/ff907fb9ab6fc54b3a01217e82d8f7a0.png"),
            new(0, "https://salt.tikicdn.com/ts/product/fd/a5/d7/4415c6f7f5ebab88256fe9afab5e7105.png"),
        };

        var attributes = BuildHomeLivingAttrs(attrMap,
            thuongHieu: "Elmich",
            xuatXu:     "Trung Quốc");

        const string description = """
            <p><img src="https://salt.tikicdn.com/ts/tmp/e4/5e/7a/7625ce96a6d0d301794c699184ab44ea.png" alt="" width="1000" height="1000" /></p>
            <p><img src="https://salt.tikicdn.com/ts/tmp/ab/2c/27/3ccda7320abf2e7867e27d637576c0a9.png" alt="" width="1000" height="1000" /></p>
            <p><img src="https://salt.tikicdn.com/ts/tmp/ab/f3/0a/aff28717fec2164f24b5aa9b645072d1.png" alt="" width="1000" height="1000" /></p>
            <p><img src="https://salt.tikicdn.com/ts/tmp/9a/3f/92/d0c7fdda484823ef5470f99ff02e6c41.png" alt="" width="1000" height="1000" /></p>
            <p><img src="https://salt.tikicdn.com/ts/tmp/9a/3f/92/a35dedb61bcdd9013eca33aea360dde7.png" alt="" width="1000" height="1000" /></p>
            <p><img src="https://salt.tikicdn.com/ts/tmp/fa/2d/9d/d784f6d307d2bf2679204f148e01d8aa.png" alt="" width="1000" height="1000" /></p>
            <p><img src="https://salt.tikicdn.com/ts/tmp/fc/4e/08/d1778fc07ea0d0e3048faab8a341cdbc.png" alt="" width="1000" height="1000" /></p>
            <p><img src="https://salt.tikicdn.com/ts/tmp/1d/a8/8c/7a48edb62732160ff8e0c994c110db38.png" alt="" width="1000" height="1000" /></p>
            <p><img src="https://salt.tikicdn.com/ts/tmp/95/76/f0/aa3325bc65f630498bad5b988faeae86.png" alt="" width="1000" height="1000" /></p>
            <p><img src="https://salt.tikicdn.com/ts/tmp/6c/95/ce/154e15f97f586fa7af592a9e7ada5b92.png" alt="" width="1000" height="1000" /></p>
            <p><img src="https://salt.tikicdn.com/ts/tmp/f8/ac/90/1bdb51d23a0735ac37eee60ac996f3b2.png" alt="" width="1000" height="1000" /></p>
            """;

        return BuildFull(
            name:             "Bình giữ nhiệt gia đình inox 304 Elmich EL8299 dung tích 900ml - Hàng chính hãng",
            slug:             "binh-giu-nhiet-gia-dinh-inox-304-elmich-el8299-dung-tich-900ml-hang-chinh-hang",
            shortDescription: "Bình giữ nhiệt gia đình Elmich EL8299 900ml, inox 304, giữ nhiệt 18 giờ.",
            skuNo:            "8600614472713",
            price:            358000m,
            sellerId:         StoreSeedData.TikiSellerId,
            images:           images,
            attributes:       attributes,
            description:      description,
            now:              now);
    }

    // ── Product 5: Bộ nồi Elmich Trimax Classic EL-2110OL ────────────────────────
    private static Product BuildBoNoiElmichTrimax(DateTimeOffset now, Dictionary<string, int> attrMap)
    {
        var images = new List<ProductImage>
        {
            new(0, "https://salt.tikicdn.com/media/catalog/producttmp/f7/83/26/63c82f21261db908c65d8e0dad707da1.png"),
            new(0, "https://salt.tikicdn.com/ts/product/91/3b/57/21660068c2950d2a4a6f64a278ae972a.jpg"),
            new(0, "https://salt.tikicdn.com/ts/product/e9/e8/c8/3b4fdc16c9d2dcd0a93ceda9b300b84e.jpg"),
            new(0, "https://salt.tikicdn.com/media/catalog/producttmp/76/33/0d/a482b792994b77042042a446f31408f6.jpg"),
            new(0, "https://salt.tikicdn.com/ts/product/73/cc/93/4867a8950fcd50293dc214aa1a2167ad.jpg"),
            new(0, "https://salt.tikicdn.com/ts/product/a1/d8/e1/d261f9575bd997199cd0e4b2f386fa80.jpg"),
            new(0, "https://salt.tikicdn.com/ts/product/3e/e7/85/aeb9974685f6fc832ed7fd2d65c16001.jpg"),
            new(0, "https://salt.tikicdn.com/ts/product/e1/e3/33/12885c3f16e4f0b2670228ccbc2ed4cf.jpg"),
        };

        var attributes = BuildHomeLivingAttrs(attrMap,
            thuongHieu: "Elmich",
            xuatXu:     "Trung Quốc");

        const string description = """
            <p><strong>Chất liệu inox 304 cao cấp</strong></p>
            <p>Bộ nồi sử dụng chất liệu lớp trong cùng là inox 304 cao cấp an toàn cho thực phẩm. Inox 304 được khẳng định là hợp kim inox tuyệt vời, có khả năng chống oxi hóa cao. Hợp kim này hoàn toàn không thôi nhiễm các chất độc hại vào thức ăn, có khả năng chống bám bẩn tối ưu và tuyệt đối an toàn cho sức khỏe.</p>
            <p><img src="https://salt.tikicdn.com/ts/tmp/ee/90/a5/228b6a4f867887f5d35d2cd0e20bf0e9.png" alt="" width="750" height="288" /></p>
            <p><strong>Công nghệ mới 3 lớp liền thân</strong></p>
            <p>"Bí mật" của bộ nồi nằm ở cấu tạo 3 lớp liền thân dày dặn - Sự kết hợp hoàn hảo của 3 lớp kim loại để tối ưu hiệu quả của từng loại chất liệu:</p>
            <p>Trong cùng là lớp inox 304 cao cấp. Chính vì là phần tiếp xúc trực tiếp với thức ăn nên chất liệu mà Elmich sử dụng phải luôn đảm bảo an toàn cho sức khỏe cho dù được nung nóng ở nhiệt độ cao hay khi sử dụng lâu năm.</p>
            <p>Nằm chính giữa là lớp nhôm dày. Đây là chất liệu truyền - giữ nhiệt tuyệt vời. Với việc bổ sung lõi nhôm, nồi có thể phân bổ nhiệt nhanh chóng, đồng đều.</p>
            <p><img src="https://salt.tikicdn.com/ts/tmp/ba/6e/8c/49761a009db9e4c0a7d3c7fc9414268c.png" alt="" width="750" height="317" /></p>
            <p>Thực phẩm được làm nóng từ cả thành và đáy mà không lo bị cháy hay bám dính nhờ đó thức ăn chín đều, rút ngắn quá trình làm chín thực phẩm và lưu giữ được lượng vi chất tối đa trong quá trình chế biến.</p>
            <p>Lớp ngoài cùng tiếp xúc với mặt bếp là inox 430 cho hiệu suất bắt từ lên đến 98%, giúp tiết kiệm nhiên liệu tối đa khi nấu. Cũng nhờ có lớp đáy từ, bộ nồi dùng được trên mọi loại bếp bao gồm bếp từ, bếp hồng ngoại, bếp gas,…</p>
            <p><img src="https://salt.tikicdn.com/ts/tmp/a4/16/46/42492fe9ec5a3b98d57537ed50b46405.png" alt="" width="750" height="324" /></p>
            <p><strong>Thiết kế tinh tế sang trọng, phù hợp với không gian bếp hiện đại</strong></p>
            <p>Diện mạo sang trọng, tinh tế: Bộ nồi inox dập nguyên khối Elmich Trimax sở hữu diện mạo sang trọng, chuẩn châu Âu với chất liệu inox sáng bóng, thiết kế quai cán thanh mảnh, tinh tế.</p>
            <p><img src="https://salt.tikicdn.com/ts/tmp/b9/4d/5b/5657c720d8a74e31c9d2259af526ea8f.png" alt="" width="750" height="406" /></p>
            <p><strong>Lỗ thoát khí lớn, viền vung rộng chống trào hiệu quả</strong></p>
            <p>Phần kính cường lực chịu nhiệt, chịu lực dày giúp bạn dễ dàng quan sát thực phẩm khi nấu. Lỗ thoát hơi lớn giúp chống trào hiệu quả, ngăn bắn nước và hơi nóng khi nấu.</p>
            <p><img src="https://salt.tikicdn.com/ts/tmp/fe/4e/d0/6574f5a54da8eb951db8c5832f73d2fa.png" alt="" width="750" height="313" /></p>
            <p>Đa dạng kích cỡ, đáp ứng mọi nhu cầu đun nấu: Bộ sản phẩm phù hợp với nhiều nhu cầu nấu nướng cùng lúc trong bữa ăn nhờ đó tiết kiệm thời gian và nhiên liệu tối đa.</p>
            """;

        return BuildFull(
            name:             "Bộ nồi Inox dập nguyên khối Elmich Trimax Classic EL-2110OL Size 18, 20, 24, chảo 26cm",
            slug:             "bo-noi-inox-dap-nguyen-khoi-elmich-trimax-classic-el-2110ol-size-18-20-24-chao-26cm",
            shortDescription: "Bộ nồi inox Elmich Trimax Classic, 4 món, đáy 3 lớp dùng được bếp từ.",
            skuNo:            "2566430088910",
            price:            2059000m,
            sellerId:         StoreSeedData.TikiSellerId,
            images:           images,
            attributes:       attributes,
            description:      description,
            now:              now);
    }

    // ── Product 6: Miếng rửa chén Scrub Daddy ────────────────────────────────────
    private static Product BuildScrubDaddy(DateTimeOffset now, Dictionary<string, int> attrMap)
    {
        var images = new List<ProductImage>
        {
            new(0, "https://salt.tikicdn.com/ts/product/f5/a4/76/b640154bcbead272f5785380fd6c0599.png"),
        };

        var attributes = BuildHomeLivingAttrs(attrMap,
            thuongHieu: "Scrub Daddy",
            xuatXu:     "Việt Nam");

        const string description = """
            <p class="QN2lPu">Miếng rửa chén bọt biển Scrub Daddy nguyên bản, miếng xốp lau chùi đa năng</p>
            <p class="QN2lPu">THÔNG TIN CHI TIẾT</p>
            <p class="QN2lPu">- Thương hiệu: Scrub Daddy</p>
            <p class="QN2lPu">- Chất liệu: FlexTexture</p>
            <p class="QN2lPu">- Xuất xứ: Việt Nam</p>
            <p class="QN2lPu">- Nguyên vật liệu: Mỹ ( USA )</p>
            <p class="QN2lPu">VỀ THƯƠNG HIỆU SCRUB DADDY:</p>
            <p class="QN2lPu">- Năm 2012, Scrub Daddy xuất hiện lần đầu tiên trên Shark Tank mùa 4 và huy động thành công 200.000 USD (4,6 tỷ) từ nhà đầu tư Lori Greiner. Hiện công ty này có giá trị lên tới trăm triệu USD và đang nắm giữ danh hiệu startup thành công nhất của Shark Tank Mỹ với việc bán được hơn 25 triệu sản phẩm.</p>
            <p><img src="https://salt.tikicdn.com/ts/tmp/e5/91/e5/efc741f3677c9196e054e6c9c4fc0713.png" alt="" width="789" height="394" /></p>
            <p class="QN2lPu">MÔ TẢ:</p>
            <p class="QN2lPu">- Scrub Daddy Original là miếng bọt biển vệ sinh được làm từ FlexTexture. Một vật liệu độc quyền có thể thay đổi kết cấu dựa trên nhiệt độ nước của bạn. Cứng trong nước lạnh để chà mạnh, mềm trong nước ấm để chà nhẹ.</p>
            <p class="QN2lPu">TÍNH NĂNG SCRUB DADDY ORIGINAL:</p>
            <p class="QN2lPu">- Rửa sạch cặn bẩn, chống mùi hôi và chỉ cần rửa bằng nước là sạch</p>
            <p class="QN2lPu">- Thiết kế tròn kết hợp cùng đôi mắt giúp giữ chặt ngón tay để bạn dễ dàng di chuyển xuống đáy và cùng lúc làm sạch cả thành bên. Miệng cười giúp rửa sạch đồng thời cả 2 mặt của đồ dùng</p>
            <p class="QN2lPu">- Để khử trùng Scrub Daddy – Có thể rửa bằng máy rửa chén trên cùng hoặc làm ẩm và cho vào lò vi sóng trong 1 phút</p>
            <p class="QN2lPu">- Không làm trầy xước trên hơn 20 bề mặt</p>
            <p><img src="https://salt.tikicdn.com/ts/tmp/38/fe/f6/9f0b37ca7b2242b22f2cb719b039c7fb.png" alt="" width="848" height="245" /></p>
            """;

        return BuildFull(
            name:             "Miếng rửa chén bọt biển Scrub Daddy nguyên bản, miếng xốp lau chùi đa năng",
            slug:             "mieng-rua-chen-bot-bien-scrub-daddy-nguyen-ban-mieng-xop-lau-chui-da-nang",
            shortDescription: "Miếng rửa chén Scrub Daddy FlexTexture, không mùi, không trầy xước, 4 màu.",
            skuNo:            "7715635354376",
            price:            119000m,
            sellerId:         StoreSeedData.TikiSellerId,
            images:           images,
            attributes:       attributes,
            description:      description,
            now:              now);
    }

    // ── Attribute helper ─────────────────────────────────────────────────────────
    private static List<ProductAttribute> BuildHomeLivingAttrs(
        Dictionary<string, int> attrMap,
        string? thuongHieu = null,
        string? xuatXu = null)
    {
        var attributes = new List<ProductAttribute>();
        void Add(string name, string? value)
        {
            if (value is null) return;
            if (attrMap.TryGetValue(name, out var id))
                attributes.Add(new ProductAttribute(id, freeTextValue: value));
        }

        Add("Thương hiệu",       thuongHieu);
        Add("Xuất xứ (Made in)", xuatXu);
        return attributes;
    }

    // ── Full builder (with images, attrs, HTML description) ──────────────────────
    private static Product BuildFull(
        string name, string slug, string shortDescription, string skuNo, decimal price,
        Guid sellerId, List<ProductImage> images, List<ProductAttribute> attributes,
        string description, DateTimeOffset now)
    {
        var categories = new List<ProductCategory> { new(HomeLivingCategoryId) };

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

    // ── Simple builder (for products 7-10 without detailed data) ─────────────────
    private static Product Build(
        string name, string slug, string description, string shortDescription,
        string skuNo, decimal price, string thumbnailUrl, DateTimeOffset createdAt)
    {
        var categories = new List<ProductCategory> { new(HomeLivingCategoryId) };
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
        typeof(Product).GetProperty("SellerId")?.SetValue(product, StoreSeedData.TikiSellerId);
        typeof(Product).GetProperty("Featured")?.SetValue(product, false);
        typeof(Product).GetProperty("Condition")?.SetValue(product, ProductCondition.New);

        return product;
    }
}
