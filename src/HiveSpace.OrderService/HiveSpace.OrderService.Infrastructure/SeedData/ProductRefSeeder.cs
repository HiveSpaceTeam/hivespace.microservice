using HiveSpace.Domain.Shared.Enumerations;
using HiveSpace.Infrastructure.Persistence.Seeding;
using HiveSpace.OrderService.Domain.External;
using HiveSpace.OrderService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HiveSpace.OrderService.Infrastructure.SeedData;

// Dev-bootstrap only. Ongoing sync is handled by ProductRefSyncConsumer.
internal sealed class ProductRefSeeder(OrderDbContext db, ILogger<ProductRefSeeder> logger) : ISeeder
{
    public int Order => 2;

    private static readonly (long ProductId, Guid StoreId, string Name, string ThumbnailUrl)[] ProductSeeds =
    [
        // Bookstore
        (1001L, new Guid("e5f6a7b8-c9d0-1234-ef01-234567890123"), "Ai Tang Luong Cho Ban?",                    "https://salt.tikicdn.com/ts/product/9b/b4/9c/5a987bea2919a2142f5db4f77c3fb5e7.png"),
        (1002L, new Guid("e5f6a7b8-c9d0-1234-ef01-234567890123"), "Song Doi Rung Rinh Thong Dong",              "https://salt.tikicdn.com/ts/product/a5/33/3b/7df8544dc35fe951c9f1e8d7e11af086.png"),
        (1003L, new Guid("f6a7b8c9-d0e1-2345-f012-345678901234"), "Combo Dan Ong Sao Hoa + Lay Tinh Tham",      "https://salt.tikicdn.com/ts/product/ea/f7/82/a783af353ebf7dd91319b414b266c3b8.jpg"),
        (1004L, new Guid("f6a7b8c9-d0e1-2345-f012-345678901234"), "Combo 3 cuon OSHO",                          "https://salt.tikicdn.com/ts/product/5c/2c/6e/b132dd51b7db43b8a9b2694d0b7b609f.jpg"),
        (1005L, new Guid("e5f6a7b8-c9d0-1234-ef01-234567890123"), "Combo To Hoc Lap Trinh",                     "https://salt.tikicdn.com/ts/product/52/ed/fb/484f032ea38a92bb080d8211ae55a039.jpg"),
        (1006L, new Guid("e5f6a7b8-c9d0-1234-ef01-234567890123"), "La Hoa Tren Duong Ve",                       "https://salt.tikicdn.com/ts/product/15/8d/9a/6cdf5d8b69e75654e892dbdb3a94167b.jpg"),
        (1007L, new Guid("f6a7b8c9-d0e1-2345-f012-345678901234"), "Tham Tu Lung Danh Conan Tap 107",            "https://salt.tikicdn.com/ts/product/5b/8c/d8/760d6721d631b232b540673126d238a2.jpg"),
        (1008L, new Guid("f6a7b8c9-d0e1-2345-f012-345678901234"), "Sach To Mau Phat Trien Tri Nao",             "https://salt.tikicdn.com/ts/product/11/d7/be/894c2ecc2d617695ad920f21e3abcae7.jpg"),
        (1009L, new Guid("f6a7b8c9-d0e1-2345-f012-345678901234"), "Monster Deluxe Edition",                     "https://salt.tikicdn.com/ts/product/84/9b/c7/2f34a84035816c04f622084184aef72d.jpg"),
        (1010L, new Guid("e5f6a7b8-c9d0-1234-ef01-234567890123"), "Thuong Tien Tuu",                            "https://salt.tikicdn.com/ts/product/4a/cc/00/765d85f5afe39b8e8f5c351e7f0ac430.png"),

        // HomeLiving
        (1011L, new Guid("b2c3d4e5-f6a7-8901-bcde-f12345678901"), "Coc giu nhiet Elmich EL8345",                "https://salt.tikicdn.com/ts/product/2b/1d/bd/5423d42a062b2355da77809294fb3cb5.png"),
        (1012L, new Guid("b2c3d4e5-f6a7-8901-bcde-f12345678901"), "Binh giu nhiet Elmich EL8295",               "https://salt.tikicdn.com/ts/product/b6/fe/b4/165dc71a83cea9aaaf58e002cd446722.jpg"),
        (1013L, new Guid("b2c3d4e5-f6a7-8901-bcde-f12345678901"), "Chao chong dinh Elmich EL5972",              "https://salt.tikicdn.com/ts/product/4b/00/0a/a8078f70ef255fc82e613468f35bcf8b.jpg"),
        (1014L, new Guid("b2c3d4e5-f6a7-8901-bcde-f12345678901"), "Binh giu nhiet Elmich EL8299",               "https://salt.tikicdn.com/ts/product/2c/41/54/b7d3d8acdcfa218a1180dfc271272788.png"),
        (1015L, new Guid("b2c3d4e5-f6a7-8901-bcde-f12345678901"), "Bo noi Inox Elmich Trimax",                  "https://salt.tikicdn.com/media/catalog/producttmp/f7/83/26/63c82f21261db908c65d8e0dad707da1.png"),
        (1016L, new Guid("b2c3d4e5-f6a7-8901-bcde-f12345678901"), "Mieng rua chen Scrub Daddy",                 "https://salt.tikicdn.com/ts/product/f5/a4/76/b640154bcbead272f5785380fd6c0599.png"),
        (1017L, new Guid("b2c3d4e5-f6a7-8901-bcde-f12345678901"), "[NEW] Coc giu nhiet Elmich EL8345",          "https://salt.tikicdn.com/cache/280x280/ts/product/4e/a9/b9/ee2f1329eda07bff45b944761ca34ff4.jpg"),
        (1018L, new Guid("b2c3d4e5-f6a7-8901-bcde-f12345678901"), "Mieng bot bien Damp Duster",                 "https://salt.tikicdn.com/cache/280x280/ts/product/a1/4a/88/ea52142a5f504d93b68b29c2869f4ba9.png"),
        (1019L, new Guid("b2c3d4e5-f6a7-8901-bcde-f12345678901"), "Diet chuot Storm",                           "https://salt.tikicdn.com/cache/280x280/ts/product/03/4f/2d/653c61f3f5c0175d2ee4cb6502f94998.png"),
        (1020L, new Guid("b2c3d4e5-f6a7-8901-bcde-f12345678901"), "Mang boc thuc pham PE Ringo",                "https://salt.tikicdn.com/cache/280x280/ts/product/18/be/5e/e46574c015e92545c691e36e11df2c5e.jpg"),

        // MobileTablet
        (1021L, new Guid("b2c3d4e5-f6a7-8901-bcde-f12345678901"), "Samsung Galaxy A07 5G",                      "https://salt.tikicdn.com/ts/product/9b/6f/bd/ee72cd139b8b14c013092590ea91438f.jpg"),
        (1022L, new Guid("b2c3d4e5-f6a7-8901-bcde-f12345678901"), "Samsung Galaxy A26 5G",                      "https://salt.tikicdn.com/ts/product/11/c0/90/7ed78d65848ac95cac6cf6c03b6edcd0.jpg"),
        (1023L, new Guid("b2c3d4e5-f6a7-8901-bcde-f12345678901"), "Cu Sac Samsung 25W",                         "https://salt.tikicdn.com/ts/product/28/e4/7f/d1f6c5467529914727d0d213b507a0c6.png"),
        (1024L, new Guid("b2c3d4e5-f6a7-8901-bcde-f12345678901"), "Samsung Galaxy Tab A11 WiFi",                "https://salt.tikicdn.com/ts/product/c8/0d/8a/eca6b95e78cbeb16863a2bedbf8f5691.png"),
        (1025L, new Guid("b2c3d4e5-f6a7-8901-bcde-f12345678901"), "Samsung Galaxy A37 5G",                      "https://salt.tikicdn.com/ts/product/ff/10/ae/26b65b7e9ddae0c4e4514dd5d0ef7ef2.jpg"),
        (1026L, new Guid("b2c3d4e5-f6a7-8901-bcde-f12345678901"), "Samsung Galaxy A16 5G",                      "https://salt.tikicdn.com/ts/product/1f/eb/e3/70ea444c1d20fd520feeb2950e11d95c.png"),
        (1027L, new Guid("b2c3d4e5-f6a7-8901-bcde-f12345678901"), "Apple iPhone 17e",                           "https://salt.tikicdn.com/ts/product/7b/51/47/5c76f01762ad765dec469005ab96833a.jpg"),
        (1028L, new Guid("b2c3d4e5-f6a7-8901-bcde-f12345678901"), "Samsung Galaxy S26 Ultra",                   "https://salt.tikicdn.com/cache/280x280/ts/product/43/51/c7/591084ec0b8bbac62ec5458dadf251f8.jpg"),
        (1029L, new Guid("b2c3d4e5-f6a7-8901-bcde-f12345678901"), "Samsung Galaxy S26",                         "https://salt.tikicdn.com/cache/280x280/ts/product/8e/49/54/7cb00769381d688d3540d20c85688c7d.jpg"),
    ];

    private static readonly (long Id, long ProductId, string SkuNo, long Price, string ImageUrl, string Attributes)[] SkuSeeds =
    [
        // Bookstore (10)
        (10001L, 1001L, "3251356516650", 149000L,   "https://salt.tikicdn.com/ts/product/9b/b4/9c/5a987bea2919a2142f5db4f77c3fb5e7.png", "{}"),
        (10002L, 1002L, "7847043795900", 143200L,   "https://salt.tikicdn.com/ts/product/a5/33/3b/7df8544dc35fe951c9f1e8d7e11af086.png", "{}"),
        (10003L, 1003L, "5143054723374", 250500L,   "https://salt.tikicdn.com/ts/product/ea/f7/82/a783af353ebf7dd91319b414b266c3b8.jpg", "{}"),
        (10004L, 1004L, "2352229843300", 302700L,   "https://salt.tikicdn.com/ts/product/5c/2c/6e/b132dd51b7db43b8a9b2694d0b7b609f.jpg", "{}"),
        (10005L, 1005L, "8070440105711", 156000L,   "https://salt.tikicdn.com/ts/product/52/ed/fb/484f032ea38a92bb080d8211ae55a039.jpg", "{}"),
        (10006L, 1006L, "8836034836300", 105840L,   "https://salt.tikicdn.com/ts/product/15/8d/9a/6cdf5d8b69e75654e892dbdb3a94167b.jpg", "{}"),
        (10007L, 1007L, "8740385161745", 25000L,    "https://salt.tikicdn.com/ts/product/5b/8c/d8/760d6721d631b232b540673126d238a2.jpg", "{}"),
        (10008L, 1008L, "9586614921348", 12320L,    "https://salt.tikicdn.com/ts/product/11/d7/be/894c2ecc2d617695ad920f21e3abcae7.jpg", "{}"),
        (10009L, 1009L, "8273928944021", 118750L,   "https://salt.tikicdn.com/ts/product/84/9b/c7/2f34a84035816c04f622084184aef72d.jpg", "{}"),
        (10010L, 1010L, "9736885668230", 118800L,   "https://salt.tikicdn.com/ts/product/4a/cc/00/765d85f5afe39b8e8f5c351e7f0ac430.png", "{}"),

        // HomeLiving (10)
        (10011L, 1011L, "9916475726151", 149000L,   "https://salt.tikicdn.com/ts/product/2b/1d/bd/5423d42a062b2355da77809294fb3cb5.png",    "{}"),
        (10012L, 1012L, "6994556055959", 147000L,   "https://salt.tikicdn.com/ts/product/b6/fe/b4/165dc71a83cea9aaaf58e002cd446722.jpg",    "{}"),
        (10013L, 1013L, "9396908958144", 212000L,   "https://salt.tikicdn.com/ts/product/4b/00/0a/a8078f70ef255fc82e613468f35bcf8b.jpg",    "{}"),
        (10014L, 1014L, "8600614472713", 358000L,   "https://salt.tikicdn.com/ts/product/2c/41/54/b7d3d8acdcfa218a1180dfc271272788.png",    "{}"),
        (10015L, 1015L, "2566430088910", 2059000L,  "https://salt.tikicdn.com/media/catalog/producttmp/f7/83/26/63c82f21261db908c65d8e0dad707da1.png", "{}"),
        (10016L, 1016L, "7715635354376", 119000L,   "https://salt.tikicdn.com/ts/product/f5/a4/76/b640154bcbead272f5785380fd6c0599.png",    "{}"),
        (10017L, 1017L, "6400137289630", 102600L,   "https://salt.tikicdn.com/cache/280x280/ts/product/4e/a9/b9/ee2f1329eda07bff45b944761ca34ff4.jpg", "{}"),
        (10018L, 1018L, "1668242013483", 63200L,    "https://salt.tikicdn.com/cache/280x280/ts/product/a1/4a/88/ea52142a5f504d93b68b29c2869f4ba9.png", "{}"),
        (10019L, 1019L, "1831551341333", 25600L,    "https://salt.tikicdn.com/cache/280x280/ts/product/03/4f/2d/653c61f3f5c0175d2ee4cb6502f94998.png", "{}"),
        (10020L, 1020L, "7441106796389", 13600L,    "https://salt.tikicdn.com/cache/280x280/ts/product/18/be/5e/e46574c015e92545c691e36e11df2c5e.jpg", "{}"),

        // MobileTablet (22)
        (10021L, 1021L, "9677706341374", 3590000L,  "https://salt.tikicdn.com/ts/product/9b/6f/bd/ee72cd139b8b14c013092590ea91438f.jpg",  "{\"Mau\":\"Den\"}"),
        (10022L, 1021L, "7422409007473", 3590000L,  "https://salt.tikicdn.com/ts/product/0d/03/5f/077cf1936d239085c90b1ddf633fe615.jpg",  "{\"Mau\":\"Xanh\"}"),
        (10023L, 1022L, "4276090513087", 6990000L,  "https://salt.tikicdn.com/ts/product/11/c0/90/7ed78d65848ac95cac6cf6c03b6edcd0.jpg",  "{\"Mau\":\"Xanh\"}"),
        (10024L, 1022L, "4937698595186", 6990000L,  "https://salt.tikicdn.com/ts/product/6d/5f/6c/a8a727e8a5e3bc04f997f4394b44a675.jpg",  "{\"Mau\":\"Den\"}"),
        (10025L, 1022L, "5458430373072", 6990000L,  "https://salt.tikicdn.com/ts/product/22/d5/e5/aa6d5a53a27f34a634ee0b51cd64a3f0.jpg",  "{\"Mau\":\"Cam\"}"),
        (10026L, 1023L, "7768310632659", 540000L,   "https://salt.tikicdn.com/ts/product/28/e4/7f/d1f6c5467529914727d0d213b507a0c6.png",  "{\"Mau\":\"Trang\"}"),
        (10027L, 1024L, "1517847354506", 3990000L,  "https://salt.tikicdn.com/ts/product/c8/0d/8a/eca6b95e78cbeb16863a2bedbf8f5691.png",  "{\"MauSac\":\"Bac\"}"),
        (10028L, 1025L, "1807041004396", 10990000L, "https://salt.tikicdn.com/ts/product/ff/10/ae/26b65b7e9ddae0c4e4514dd5d0ef7ef2.jpg",  "{\"MauSac\":\"Xanh\"}"),
        (10029L, 1025L, "6116779689074", 10990000L, "https://salt.tikicdn.com/ts/product/6a/63/e2/b5cacfcd9cd46b01acf952837fcb09b8.jpg",  "{\"MauSac\":\"Den\"}"),
        (10030L, 1025L, "7416742245238", 10990000L, "https://salt.tikicdn.com/ts/product/9b/28/00/9e79c4f0ec2aba5815d92182a6c17dab.jpg",  "{\"MauSac\":\"Trang\"}"),
        (10031L, 1025L, "3862966111725", 10990000L, "https://salt.tikicdn.com/ts/product/ce/7f/e3/60dd7a468610770f492349f10c8f6962.jpg",  "{\"MauSac\":\"Tim\"}"),
        (10032L, 1026L, "9356381205803", 4990000L,  "https://salt.tikicdn.com/ts/product/1f/eb/e3/70ea444c1d20fd520feeb2950e11d95c.png",  "{\"MauSac\":\"Den\"}"),
        (10033L, 1027L, "8390602435727", 16990000L, "https://salt.tikicdn.com/ts/product/7b/51/47/5c76f01762ad765dec469005ab96833a.jpg",  "{\"Mau\":\"Trang\"}"),
        (10034L, 1027L, "9989498460265", 16990000L, "https://salt.tikicdn.com/ts/product/c1/99/ce/7ee3dad9756ea2122ffe5040c66f73dc.jpg",  "{\"Mau\":\"Den\"}"),
        (10035L, 1027L, "6612557182714", 16990000L, "https://salt.tikicdn.com/ts/product/fb/16/51/9a75ea240dd4dc37a30fef4e877cdac0.jpg",  "{\"Mau\":\"Hong\"}"),
        (10036L, 1028L, "5133113695023", 36990000L, "https://salt.tikicdn.com/cache/280x280/ts/product/43/51/c7/591084ec0b8bbac62ec5458dadf251f8.jpg", "{\"MauSac\":\"Trang\"}"),
        (10037L, 1028L, "8308809436012", 36990000L, "https://salt.tikicdn.com/cache/280x280/ts/product/43/51/c7/cb1cb2583bc8c5d5ea0c9ed4a1f0394f.jpg", "{\"MauSac\":\"Den\"}"),
        (10038L, 1028L, "2102639080540", 36990000L, "https://salt.tikicdn.com/cache/280x280/ts/product/43/51/c7/3561261ed1e13d4fe0daf97d5e2483e7.jpg", "{\"MauSac\":\"Tim\"}"),
        (10039L, 1029L, "2565010916174", 25990000L, "https://salt.tikicdn.com/cache/280x280/ts/product/8e/49/54/7cb00769381d688d3540d20c85688c7d.jpg", "{\"MauSac\":\"Trang\"}"),
        (10040L, 1029L, "7727270065680", 25990000L, "https://salt.tikicdn.com/cache/280x280/ts/product/8e/49/54/08c6ab66027620ef231fe7142c578536.jpg", "{\"MauSac\":\"Tim\"}"),
        (10041L, 1029L, "5803096522639", 25990000L, "https://salt.tikicdn.com/cache/280x280/ts/product/8e/49/54/a8001b59534674a9e60d1e9aade38879.jpg", "{\"MauSac\":\"Xanh\"}"),
        (10042L, 1029L, "4417290963754", 25990000L, "https://salt.tikicdn.com/cache/280x280/ts/product/8e/49/54/d2acbd5f1d25e3ec25f30407220cbfc4.jpg", "{\"MauSac\":\"Den\"}"),
    ];

    public async Task SeedAsync(CancellationToken ct = default)
    {
        var productIds = ProductSeeds.Select(p => p.ProductId).ToList();
        var existingProducts = await db.ProductRefs
            .Where(p => productIds.Contains(p.Id))
            .Select(p => p.Id)
            .ToHashSetAsync(ct);

        var skuIds = SkuSeeds.Select(s => s.Id).ToList();
        var existingSkus = await db.SkuRefs
            .Where(s => skuIds.Contains(s.Id))
            .Select(s => s.Id)
            .ToHashSetAsync(ct);

        var productsToAdd = ProductSeeds.Where(p => !existingProducts.Contains(p.ProductId)).ToList();
        var skusToAdd = SkuSeeds.Where(s => !existingSkus.Contains(s.Id)).ToList();

        if (productsToAdd.Count == 0 && skusToAdd.Count == 0)
        {
            logger.LogDebug("All expected ProductRefs and SkuRefs already exist. Skipping.");
            return;
        }

        var strategy = db.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            db.ChangeTracker.Clear();
            var existingProdsNow = await db.ProductRefs
                .Where(p => productIds.Contains(p.Id))
                .Select(p => p.Id)
                .ToHashSetAsync(ct);
            var existingSkusNow = await db.SkuRefs
                .Where(s => skuIds.Contains(s.Id))
                .Select(s => s.Id)
                .ToHashSetAsync(ct);

            var toAddProds = ProductSeeds.Where(p => !existingProdsNow.Contains(p.ProductId)).ToList();
            var toAddSkus = SkuSeeds.Where(s => !existingSkusNow.Contains(s.Id)).ToList();
            if (toAddProds.Count == 0 && toAddSkus.Count == 0) return;

            await using var tx = await db.Database.BeginTransactionAsync(ct);

            foreach (var (productId, storeId, name, thumbnailUrl) in toAddProds)
                db.ProductRefs.Add(new ProductRef(productId, storeId, name, thumbnailUrl, ProductStatus.Available));

            foreach (var (id, productId, skuNo, price, imageUrl, attributes) in toAddSkus)
                db.SkuRefs.Add(new SkuRef(id, productId, skuNo, price, "VND", imageUrl, attributes));

            await db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
            logger.LogInformation("Seeded {ProductCount} ProductRef(s) and {SkuCount} SkuRef(s).", toAddProds.Count, toAddSkus.Count);
        });
    }
}
