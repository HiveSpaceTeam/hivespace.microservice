using Duende.IdentityModel;
using HiveSpace.UserService.Infrastructure.Data;
using HiveSpace.UserService.Infrastructure.Identity;
using HiveSpace.UserService.Domain.Aggregates.Store;
using HiveSpace.UserService.Domain.Aggregates.User;
using HiveSpace.UserService.Domain.Services;
using HiveSpace.UserService.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace HiveSpace.UserService.Infrastructure;

public static class SeedData
{
    public static readonly Guid AliceId = new Guid("11111111-1111-1111-1111-111111111111");
    public static readonly Guid BobId = new Guid("22222222-2222-2222-2222-222222222222");
    // public static readonly Guid SysAdminId = new Guid("33333333-3333-3333-3333-333333333333");
    // public static readonly Guid AdminId = new Guid("44444444-4444-4444-4444-444444444444");
    public static async Task EnsureSeedDataAsync(WebApplication app, CancellationToken ct = default)
    {
        await using var scope = app.Services.CreateAsyncScope();
        var context      = scope.ServiceProvider.GetRequiredService<UserDbContext>();
        var userMgr      = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var storeManager = scope.ServiceProvider.GetRequiredService<StoreManager>();
        var logger       = scope.ServiceProvider.GetRequiredService<ILogger<UserDbContext>>();

        var pending = (await context.Database.GetPendingMigrationsAsync(ct)).ToList();
        if (pending.Count > 0)
        {
            logger.LogInformation("Applying {Count} pending migration(s): {Migrations}",
                pending.Count, string.Join(", ", pending));
            await context.Database.MigrateAsync(ct);
            logger.LogInformation("Migrations applied successfully.");
        }

        await SeedAliceAsync(userMgr, context, logger, ct);
        await SeedBobAsync(userMgr, context, logger, ct);
        await SeedSystemAdminAsync(userMgr, logger, ct);
        await SeedAdminAsync(userMgr, context, logger, ct);
        await SeedSellersAsync(userMgr, storeManager, context, logger, ct);
    }

    private static async Task SeedAliceAsync(
        UserManager<ApplicationUser> userMgr, UserDbContext context,
        ILogger logger, CancellationToken ct)
    {
        var alice = await userMgr.FindByNameAsync("alice");
        if (alice != null)
        {
            logger.LogDebug("alice already exists");
            return;
        }

        alice = new ApplicationUser
        {
            Id             = AliceId,
            UserName       = "alice",
            Email          = "aliceSmith@gmail.com",
            EmailConfirmed = false,
            FullName       = "Alice Smith",
            PhoneNumber    = "+84901234567",
            DateOfBirth    = new DateTime(1990, 1, 15),
            Gender         = (int)Gender.Female,
            Status         = (int)UserStatus.Active,
            CreatedAt      = DateTimeOffset.UtcNow,
            Theme          = Theme.Light,
            Culture        = Culture.Vi
        };

        var result = await userMgr.CreateAsync(alice, "AliceSmith123$");
        if (!result.Succeeded)
        {
            logger.LogError("Failed to create alice: {Error}", result.Errors.First().Description);
            throw new Exception(result.Errors.First().Description);
        }

        var aliceHomeAddress = new Address(
            fullName: "Alice Smith", phoneNumber: "+84901234567",
            street: "123 Main Street", district: "Downtown",
            province: "California", country: "USA",
            zipCode: "12345", addressType: AddressType.Home);

        aliceHomeAddress.SetAsDefault();
        context.Entry(aliceHomeAddress).Property("UserId").CurrentValue = alice.Id;
        context.Addresses.Add(aliceHomeAddress);

        result = await userMgr.AddClaimsAsync(alice,
        [
            new Claim(JwtClaimTypes.Name,       "Alice Smith"),
            new Claim(JwtClaimTypes.GivenName,  "Alice"),
            new Claim(JwtClaimTypes.FamilyName, "Smith"),
            new Claim(JwtClaimTypes.WebSite,    "http://alice.example.com"),
            new Claim(JwtClaimTypes.Picture,    "https://scontent.fhan7-1.fna.fbcdn.net/v/t39.30808-1/529742079_777341701485971_4020105096641008833_n.jpg?stp=dst-jpg_s200x200_tt6&_nc_cat=106&ccb=1-7&_nc_sid=e99d92&_nc_eui2=AeHOZtTR0YEI1wTAoLhcXxYrZ_lDYDRtnbpn-UNgNG2dujEOqjbJP2jUyZLseGPjOw7m1we5cC62ulwjACTvYMP5&_nc_ohc=u4NsqmH7Ci0Q7kNvwF1-Wi5&_nc_oc=AdrxQ0nCifqlTK4bIxsGMxJIMtJMRNU-mIbx0fPKus5W9-JxM4vEoYZ6jbqIY4FIAIRn0jRamFEn9EGMuNuWWXTt&_nc_zt=24&_nc_ht=scontent.fhan7-1.fna&_nc_gid=_2qgRLt6g8Es9mmKIN6Vlw&_nc_ss=7a3a8&oh=00_Af02at1L9pVGXBbLQ1TXaFSwosarmT2Yv3kwgwda-cFdXg&oe=69E92E54"),
        ]);
        if (!result.Succeeded)
        {
            logger.LogError("Failed to add claims to alice: {Error}", result.Errors.First().Description);
            throw new Exception(result.Errors.First().Description);
        }

        await context.SaveChangesAsync(ct);
        logger.LogDebug("alice created with sample address");
    }

    private static async Task SeedBobAsync(
        UserManager<ApplicationUser> userMgr, UserDbContext context,
        ILogger logger, CancellationToken ct)
    {
        var bob = await userMgr.FindByNameAsync("bob");
        if (bob != null)
        {
            logger.LogDebug("bob already exists");
            return;
        }

        bob = new ApplicationUser
        {
            Id             = BobId,
            UserName       = "bob",
            Email          = "bobSmith@gmail.com",
            EmailConfirmed = false,
            FullName       = "Bob Smith",
            PhoneNumber    = "+84987654321",
            DateOfBirth    = new DateTime(1985, 6, 20),
            Gender         = (int)Gender.Male,
            Status         = (int)UserStatus.Active,
            CreatedAt      = DateTimeOffset.UtcNow,
            Theme          = Theme.Light,
            Culture        = Culture.Vi
        };

        var result = await userMgr.CreateAsync(bob, "BobSmith123$");
        if (!result.Succeeded)
        {
            logger.LogError("Failed to create bob: {Error}", result.Errors.First().Description);
            throw new Exception(result.Errors.First().Description);
        }

        var bobHomeAddress = new Address(
            fullName: "Bob Smith", phoneNumber: "+84987654321",
            street: "456 Oak Avenue", district: "Suburb",
            province: "Texas", country: "USA",
            zipCode: "67890", addressType: AddressType.Home);

        var bobWorkAddress = new Address(
            fullName: "Bob Smith", phoneNumber: "+84987654321",
            street: "789 Business Blvd", district: "Business District",
            province: "Texas", country: "USA",
            zipCode: "67891", addressType: AddressType.Work);

        bobHomeAddress.SetAsDefault();
        context.Entry(bobHomeAddress).Property("UserId").CurrentValue = bob.Id;
        context.Entry(bobWorkAddress).Property("UserId").CurrentValue = bob.Id;
        context.Addresses.AddRange(bobHomeAddress, bobWorkAddress);

        result = await userMgr.AddClaimsAsync(bob,
        [
            new Claim(JwtClaimTypes.Name,       "Bob Smith"),
            new Claim(JwtClaimTypes.GivenName,  "Bob"),
            new Claim(JwtClaimTypes.FamilyName, "Smith"),
            new Claim(JwtClaimTypes.WebSite,    "http://bob.example.com"),
            new Claim("location",               "somewhere"),
            new Claim(JwtClaimTypes.Picture,    "https://scontent.fhan7-1.fna.fbcdn.net/v/t39.30808-1/619108846_2920789344795889_1339672122962744350_n.jpg?stp=dst-jpg_s200x200_tt6&_nc_cat=108&ccb=1-7&_nc_sid=e99d92&_nc_eui2=AeGRyZ28lkuXPO966VWTa7-WXJ5jeRHvuA1cnmN5Ee-4DdkXY30zOg5G3Xv5E85oBsdjDBCC81n8TavngkrBmWN7&_nc_ohc=DKnUNDzXPSsQ7kNvwFeEkU5&_nc_oc=AdqslpzU_dqjl89K9X3s70qGjddv8Pi7HzRhAwpQifj7tTJIsSxyG_Ro__Yt2IR87m3-3tCirslSOKeotp7gGJMT&_nc_zt=24&_nc_ht=scontent.fhan7-1.fna&_nc_gid=NgW59NCpuV6NO_P9hLKcew&_nc_ss=7a3a8&oh=00_Af1SoYAMjpm3uOR1QS87h6G3obpvpSgeq6gMaaE3MCDTYQ&oe=69E945C0"),
        ]);
        if (!result.Succeeded)
        {
            logger.LogError("Failed to add claims to bob: {Error}", result.Errors.First().Description);
            throw new Exception(result.Errors.First().Description);
        }

        await context.SaveChangesAsync(ct);
        logger.LogDebug("bob created with sample addresses");
    }

    private static async Task SeedSystemAdminAsync(
        UserManager<ApplicationUser> userMgr, ILogger logger, CancellationToken ct)
    {
        var systemAdmin = await userMgr.FindByNameAsync("sysadmin");
        if (systemAdmin != null)
        {
            logger.LogDebug("sysadmin already exists");
            return;
        }

        systemAdmin = new ApplicationUser
        {
            UserName       = "sysadmin",
            Email          = "sysadmin@hivespace.com",
            EmailConfirmed = true,
            FullName       = "System Administrator",
            PhoneNumber    = "+84911111111",
            DateOfBirth    = new DateTime(1980, 3, 10),
            Gender         = (int)Gender.Male,
            Status         = (int)UserStatus.Active,
            RoleName       = "SystemAdmin",
            CreatedAt      = DateTimeOffset.UtcNow,
            Theme          = Theme.Light,
            Culture        = Culture.Vi
        };

        var result = await userMgr.CreateAsync(systemAdmin, "SysAdmin123$");
        if (!result.Succeeded)
        {
            logger.LogError("Failed to create sysadmin: {Error}", result.Errors.First().Description);
            throw new Exception(result.Errors.First().Description);
        }

        await userMgr.AddClaimsAsync(systemAdmin,
        [
            new Claim(JwtClaimTypes.Name,       "System Administrator"),
            new Claim(JwtClaimTypes.GivenName,  "System"),
            new Claim(JwtClaimTypes.FamilyName, "Administrator"),
            new Claim(JwtClaimTypes.Role,       "SystemAdmin"),
            new Claim("permissions",            "system.manage"),
            new Claim("permissions",            "user.manage"),
            new Claim("permissions",            "store.manage"),
        ]);

        logger.LogDebug("sysadmin created");
    }

    private static async Task SeedAdminAsync(
        UserManager<ApplicationUser> userMgr, UserDbContext context,
        ILogger logger, CancellationToken ct)
    {
        var admin = await userMgr.FindByNameAsync("admin");
        if (admin != null)
        {
            logger.LogDebug("admin already exists");
            return;
        }

        admin = new ApplicationUser
        {
            UserName       = "admin",
            Email          = "admin@hivespace.com",
            EmailConfirmed = true,
            FullName       = "Admin User",
            PhoneNumber    = "+84922222222",
            DateOfBirth    = new DateTime(1985, 8, 22),
            Gender         = (int)Gender.Female,
            Status         = (int)UserStatus.Active,
            RoleName       = "Admin",
            CreatedAt      = DateTimeOffset.UtcNow,
            Theme          = Theme.Light,
            Culture        = Culture.Vi
        };

        var result = await userMgr.CreateAsync(admin, "Admin123$");
        if (!result.Succeeded)
        {
            logger.LogError("Failed to create admin: {Error}", result.Errors.First().Description);
            throw new Exception(result.Errors.First().Description);
        }

        var adminAddress = new Address(
            fullName: "Admin User", phoneNumber: "+84922222222",
            street: "100 Admin Plaza", district: "Central",
            province: "New York", country: "USA",
            zipCode: "10001", addressType: AddressType.Work);

        adminAddress.SetAsDefault();
        context.Entry(adminAddress).Property("UserId").CurrentValue = admin.Id;
        context.Addresses.Add(adminAddress);

        await userMgr.AddClaimsAsync(admin,
        [
            new Claim(JwtClaimTypes.Name,       "Admin User"),
            new Claim(JwtClaimTypes.GivenName,  "Admin"),
            new Claim(JwtClaimTypes.FamilyName, "User"),
            new Claim(JwtClaimTypes.Role,       "Admin"),
            new Claim("permissions",            "user.manage"),
            new Claim("permissions",            "store.view"),
        ]);

        await context.SaveChangesAsync(ct);
        logger.LogDebug("admin created with sample address");
    }

    private static async Task SeedSellersAsync(
        UserManager<ApplicationUser> userMgr, StoreManager storeManager,
        UserDbContext context, ILogger logger, CancellationToken ct)
    {
        var sellerSeeds = new[]
        {
            new
            {
                SellerId     = new Guid("a1b2c3d4-e5f6-7890-abcd-ef1234567890"),
                StoreId      = new Guid("b2c3d4e5-f6a7-8901-bcde-f12345678901"),
                Username     = "tiki",
                Email        = "tiki@gmail.com",
                FullName     = "Tiki Trading",
                Phone        = "+84901000001",
                DateOfBirth  = new DateTime(1988, 12, 5),
                Gender       = (int)Gender.Male,
                Password     = "TikiTrading123$",
                StoreName    = "Tiki Trading",
                StoreDescription = "OFFICIAL_STORE • 4.7 ★ (5.5tr+ đánh giá) • 513.1k+ người theo dõi",
                LogoUrl      = "https://vcdn.tikicdn.com/ts/seller/d1/3f/ae/13ce3d83ab6b6c5e77e6377ad61dc4a5.jpg",
                StoreAddress = "https://tiki.vn/cua-hang/tiki-trading",
                GivenName    = "Tiki",
                FamilyName   = "Trading"
            },
            new
            {
                SellerId     = new Guid("c3d4e5f6-a7b8-9012-cdef-012345678901"),
                StoreId      = new Guid("e5f6a7b8-c9d0-1234-ef01-234567890123"),
                Username     = "giver",
                Email        = "giver@gmail.com",
                FullName     = "GIVER BOOKS & MEDIA",
                Phone        = "+84901000002",
                DateOfBirth  = new DateTime(1989, 1, 5),
                Gender       = (int)Gender.Male,
                Password     = "GiverBooks123$",
                StoreName    = "GIVER BOOKS & MEDIA",
                StoreDescription = "OFFICIAL_STORE • 4.8 ★ (8.2k+ đánh giá) • 6.0k+ người theo dõi",
                LogoUrl      = "https://vcdn.tikicdn.com/ts/seller/89/9e/7d/d19991a65a04abc9b0a410058307d255.jpg",
                StoreAddress = "https://tiki.vn/cua-hang/giver-books",
                GivenName    = "Giver",
                FamilyName   = "Books",
            },
            new
            {
                SellerId     = new Guid("d4e5f6a7-b8c9-0123-def0-123456789012"),
                StoreId      = new Guid("f6a7b8c9-d0e1-2345-f012-345678901234"),
                Username     = "phuongdong",
                Email        = "phuongdong@gmail.com",
                FullName     = "Phương Đông Books",
                Phone        = "+84901000003",
                DateOfBirth  = new DateTime(1990, 2, 10),
                Gender       = (int)Gender.Male,
                Password     = "PhuongDongBooks123$",
                StoreName    = "Phương Đông Books",
                StoreDescription = "4.8 ★ (38k+ đánh giá) • 14.5k+ người theo dõi",
                LogoUrl      = "https://vcdn.tikicdn.com/ts/seller/2e/85/b7/e76104ae5f1beaf244f319e2f0d2d413.jpg",
                StoreAddress = "https://tiki.vn/cua-hang/phuong-dong-books",
                GivenName    = "Phuong",
                FamilyName   = "Dong"
            },
        };

        foreach (var seed in sellerSeeds)
        {
            var seller = userMgr.Users.FirstOrDefault(u => u.Id == seed.SellerId)
                      ?? userMgr.Users.FirstOrDefault(u => u.UserName == seed.Username);

            if (seller is not null && seller.Id != seed.SellerId)
            {
                logger.LogWarning(
                    "Skipping seller {Username}: username maps to different ID {ExistingId} (expected {ExpectedId}).",
                    seed.Username, seller.Id, seed.SellerId);
                continue;
            }

            if (seller is null)
            {
                seller = new ApplicationUser
                {
                    Id             = seed.SellerId,
                    UserName       = seed.Username,
                    Email          = seed.Email,
                    EmailConfirmed = false,
                    FullName       = seed.FullName,
                    PhoneNumber    = seed.Phone,
                    DateOfBirth    = seed.DateOfBirth,
                    Gender         = seed.Gender,
                    Status         = (int)UserStatus.Active,
                    RoleName       = "Seller",
                    CreatedAt      = DateTimeOffset.UtcNow,
                    Theme          = Theme.Light,
                    Culture        = Culture.Vi
                };

                var createResult = await userMgr.CreateAsync(seller, seed.Password);
                if (!createResult.Succeeded)
                {
                    logger.LogError("Failed to create seller {Username}: {Error}",
                        seed.Username, createResult.Errors.First().Description);
                    throw new Exception(createResult.Errors.First().Description);
                }
            }

            var store = context.Stores.FirstOrDefault(s => s.Id == seed.StoreId)
                     ?? context.Stores.FirstOrDefault(s => s.OwnerId == seed.SellerId);

            if (store is null)
            {
                var newStore = await storeManager.RegisterStoreAsync(
                    name:         seed.StoreName,
                    description:  seed.StoreDescription,
                    logoUrl:      seed.LogoUrl,
                    storeAddress: seed.StoreAddress,
                    ownerId:      seed.SellerId,
                    storeId:      seed.StoreId);

                context.Stores.Add(newStore);
                await context.SaveChangesAsync(ct);
                logger.LogDebug("Created store {StoreName} for seller {Username}.", seed.StoreName, seed.Username);
            }
            else if (store.OwnerId != seed.SellerId || store.Id != seed.StoreId)
            {
                logger.LogWarning(
                    "Store mismatch for {Username}. Existing store ID/OwnerId is {StoreId}/{OwnerId}, expected {ExpectedStoreId}/{ExpectedOwnerId}.",
                    seed.Username, store.Id, store.OwnerId, seed.StoreId, seed.SellerId);
            }

            if (seller.StoreId != seed.StoreId)
            {
                seller.StoreId = seed.StoreId;
                var updateResult = await userMgr.UpdateAsync(seller);
                if (!updateResult.Succeeded)
                {
                    logger.LogError("Failed to update seller {Username} with StoreId: {Error}",
                        seed.Username, updateResult.Errors.First().Description);
                    throw new Exception(updateResult.Errors.First().Description);
                }
            }

            var hasAddress = context.Addresses.Any(a => EF.Property<Guid>(a, "UserId") == seed.SellerId);
            if (!hasAddress)
            {
                var sellerAddress = new Address(
                    fullName: seed.FullName, phoneNumber: seed.Phone,
                    street: seed.StoreName, district: "District 1",
                    province: "Ho Chi Minh City", country: "Vietnam",
                    zipCode: "70000", addressType: AddressType.Work);

                sellerAddress.SetAsDefault();
                context.Entry(sellerAddress).Property("UserId").CurrentValue = seed.SellerId;
                context.Addresses.Add(sellerAddress);
                await context.SaveChangesAsync(ct);
            }

            var existingClaims = await userMgr.GetClaimsAsync(seller);
            var requiredClaims = new[]
            {
                new Claim(JwtClaimTypes.Name,       seed.FullName),
                new Claim(JwtClaimTypes.GivenName,  seed.GivenName),
                new Claim(JwtClaimTypes.FamilyName, seed.FamilyName),
                new Claim(JwtClaimTypes.Role,       "Seller"),
                new Claim("permissions",            "store.manage"),
                new Claim("permissions",            "product.manage"),
                new Claim("permissions",            "order.view"),
            };

            var claimsToAdd = requiredClaims
                .Where(c => !existingClaims.Any(ec => ec.Type == c.Type && ec.Value == c.Value))
                .ToArray();

            if (claimsToAdd.Length > 0)
            {
                var claimResult = await userMgr.AddClaimsAsync(seller, claimsToAdd);
                if (!claimResult.Succeeded)
                {
                    logger.LogError("Failed to add claims for seller {Username}: {Error}",
                        seed.Username, claimResult.Errors.First().Description);
                    throw new Exception(claimResult.Errors.First().Description);
                }
            }

            logger.LogDebug("Seller seed ensured for {Username} (SellerId={SellerId}, StoreId={StoreId}).",
                seed.Username, seed.SellerId, seed.StoreId);
        }
    }
}
