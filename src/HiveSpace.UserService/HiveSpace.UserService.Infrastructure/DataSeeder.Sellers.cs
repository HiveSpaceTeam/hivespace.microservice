using HiveSpace.UserService.Domain.Aggregates.Store;
using HiveSpace.UserService.Domain.Aggregates.User;
using HiveSpace.Domain.Shared.Enumerations;
using HiveSpace.UserService.Domain.Enums;
using HiveSpace.UserService.Domain.Services;
using HiveSpace.UserService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HiveSpace.UserService.Infrastructure;

public static partial class DataSeeder
{
    private const string TikiAvatarUrl       = "https://images.unsplash.com/photo-1595152772835-219674b2a8a6?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&ixlib=rb-4.0.3&q=80&w=1080";
    private const string GiverAvatarUrl      = "https://images.unsplash.com/photo-1596793884200-971f7b08b63a?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&ixlib=rb-4.0.3&q=80&w=1080";
    private const string PhuongDongAvatarUrl = "https://images.unsplash.com/photo-1621694691319-0d74e2d9a79c?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&ixlib=rb-4.0.3&q=80&w=1080";

    private static async Task SeedSellersAsync(
        StoreManager storeManager,
        UserDbContext context, ILogger logger, CancellationToken ct)
    {
        var sellerSeeds = new[]
        {
            new
            {
                SellerId         = new Guid("a1b2c3d4-e5f6-7890-abcd-ef1234567890"),
                StoreId          = new Guid("b2c3d4e5-f6a7-8901-bcde-f12345678901"),
                Username         = "tiki",
                Email            = "tiki@gmail.com",
                FullName         = "Tiki Trading",
                Phone            = "+84901000001",
                DateOfBirth      = new DateTime(1988, 12, 5),
                Gender           = Gender.Male,
                AvatarUrl        = TikiAvatarUrl,
                StoreName        = "Tiki Trading",
                StoreDescription = "OFFICIAL_STORE • 4.7 ★ (5.5tr+ đánh giá) • 513.1k+ người theo dõi",
                LogoUrl          = "https://vcdn.tikicdn.com/ts/seller/d1/3f/ae/13ce3d83ab6b6c5e77e6377ad61dc4a5.jpg",
                StoreAddress     = "https://tiki.vn/cua-hang/tiki-trading",
            },
            new
            {
                SellerId         = new Guid("c3d4e5f6-a7b8-9012-cdef-012345678901"),
                StoreId          = new Guid("e5f6a7b8-c9d0-1234-ef01-234567890123"),
                Username         = "giver",
                Email            = "giver@gmail.com",
                FullName         = "GIVER BOOKS & MEDIA",
                Phone            = "+84901000002",
                DateOfBirth      = new DateTime(1989, 1, 5),
                Gender           = Gender.Male,
                AvatarUrl        = GiverAvatarUrl,
                StoreName        = "GIVER BOOKS & MEDIA",
                StoreDescription = "OFFICIAL_STORE • 4.8 ★ (8.2k+ đánh giá) • 6.0k+ người theo dõi",
                LogoUrl          = "https://vcdn.tikicdn.com/ts/seller/89/9e/7d/d19991a65a04abc9b0a410058307d255.jpg",
                StoreAddress     = "https://tiki.vn/cua-hang/giver-books",
            },
            new
            {
                SellerId         = new Guid("d4e5f6a7-b8c9-0123-def0-123456789012"),
                StoreId          = new Guid("f6a7b8c9-d0e1-2345-f012-345678901234"),
                Username         = "phuongdong",
                Email            = "phuongdong@gmail.com",
                FullName         = "Phương Đông Books",
                Phone            = "+84901000003",
                DateOfBirth      = new DateTime(1990, 2, 10),
                Gender           = Gender.Male,
                AvatarUrl        = PhuongDongAvatarUrl,
                StoreName        = "Phương Đông Books",
                StoreDescription = "4.8 ★ (38k+ đánh giá) • 14.5k+ người theo dõi",
                LogoUrl          = "https://vcdn.tikicdn.com/ts/seller/2e/85/b7/e76104ae5f1beaf244f319e2f0d2d413.jpg",
                StoreAddress     = "https://tiki.vn/cua-hang/phuong-dong-books",
            },
        };

        foreach (var seed in sellerSeeds)
        {
            var seller = await context.Users.Include(u => u.Addresses).FirstOrDefaultAsync(u => u.Id == seed.SellerId, ct)
                      ?? await context.Users.Include(u => u.Addresses).FirstOrDefaultAsync(u => u.UserName == seed.Username, ct);

            if (seller is not null && seller.Id != seed.SellerId)
            {
                logger.LogWarning(
                    "Skipping seller {Username}: username maps to different ID {ExistingId} (expected {ExpectedId}).",
                    seed.Username, seller.Id, seed.SellerId);
                continue;
            }

            if (seller is null)
            {
                seller = User.CreateProfile(
                    id: seed.SellerId,
                    email: Email.Create(seed.Email),
                    userName: seed.Username,
                    fullName: seed.FullName,
                    avatarUrl: seed.AvatarUrl,
                    phoneNumber: PhoneNumber.CreateOrDefault(seed.Phone),
                    dateOfBirth: DateOfBirth.CreateOrDefault(seed.DateOfBirth),
                    gender: seed.Gender,
                    createdAt: DateTimeOffset.UtcNow
                );
                
                seller.AddAddress(
                    fullName: seed.FullName, phoneNumber: seed.Phone,
                    street: seed.StoreName, commune: "District 1",
                    province: "Ho Chi Minh City", country: "Vietnam",
                    zipCode: "70000", addressType: AddressType.Work,
                    setAsDefault: true);

                context.Users.Add(seller);
                await context.SaveChangesAsync(ct);
            }
            else
            {
                if (seller.AvatarUrl != seed.AvatarUrl)
                {
                    seller.SetAvatarUrl(seed.AvatarUrl);
                    await context.SaveChangesAsync(ct);
                }
            }

            var store = await context.Stores.FirstOrDefaultAsync(s => s.Id == seed.StoreId, ct)
                     ?? await context.Stores.FirstOrDefaultAsync(s => s.OwnerId == seed.SellerId, ct);

            if (store is null)
            {
                var registration = await storeManager.RegisterStoreAsync(
                    name:         seed.StoreName,
                    description:  seed.StoreDescription,
                    logoFileId:   Guid.NewGuid().ToString(),
                    storeAddress: seed.StoreAddress,
                    ownerId:      seed.SellerId,
                    storeId:      seed.StoreId);

                registration.Store.SetLogoUrl(seed.LogoUrl);
                context.Stores.Add(registration.Store);
                await context.SaveChangesAsync(ct);
                logger.LogDebug("Created store {StoreName} for seller {Username}.", seed.StoreName, seed.Username);
            }
            else if (store.OwnerId != seed.SellerId || store.Id != seed.StoreId)
            {
                logger.LogWarning(
                    "Store mismatch for {Username}. Existing store ID/OwnerId is {StoreId}/{OwnerId}, expected {ExpectedStoreId}/{ExpectedOwnerId}.",
                    seed.Username, store.Id, store.OwnerId, seed.StoreId, seed.SellerId);
            }

            logger.LogDebug("Seller seed ensured for {Username} (SellerId={SellerId}, StoreId={StoreId}).",
                seed.Username, seed.SellerId, seed.StoreId);
        }
    }
}
