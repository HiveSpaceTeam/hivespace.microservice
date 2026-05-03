using Duende.IdentityModel;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.UserService.Domain.Aggregates.Store;
using HiveSpace.UserService.Domain.Aggregates.User;
using HiveSpace.UserService.Domain.Enums;
using HiveSpace.UserService.Domain.Exceptions;
using HiveSpace.UserService.Domain.Services;
using HiveSpace.UserService.Infrastructure.Data;
using HiveSpace.UserService.Infrastructure.Identity;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace HiveSpace.UserService.Infrastructure;

public static class DataSeeder
{
    public static readonly Guid AliceId = new Guid("11111111-1111-1111-1111-111111111111");
    public static readonly Guid BobId = new Guid("22222222-2222-2222-2222-222222222222");
    private const string AliceAvatarUrl = "https://cdn.discordapp.com/avatars/474579515188707339/24acc2c6b645216504447360a58c0683.webp";
    private const string BobAvatarUrl = "https://cdn.discordapp.com/avatars/743061397037908059/edaa33d7f618405017a26cc7ca42379b.webp";
    private const string SystemAdminAvatarUrl = "https://images.unsplash.com/photo-1500648767791-00dcc994a43e?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&ixlib=rb-4.0.3&q=80&w=1080";
    private const string AdminAvatarUrl = "https://images.unsplash.com/photo-1494790108377-be9c29b29330?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&ixlib=rb-4.0.3&q=80&w=1080";
    private const string TikiAvatarUrl = "https://images.unsplash.com/photo-1595152772835-219674b2a8a6?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&ixlib=rb-4.0.3&q=80&w=1080";
    private const string GiverAvatarUrl = "https://images.unsplash.com/photo-1596793884200-971f7b08b63a?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&ixlib=rb-4.0.3&q=80&w=1080";
    private const string PhuongDongAvatarUrl = "https://images.unsplash.com/photo-1621694691319-0d74e2d9a79c?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&ixlib=rb-4.0.3&q=80&w=1080";
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
        await SeedSystemAdminAsync(userMgr, context, logger, ct);
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
            if (await EnsureAvatarUrlAsync(userMgr, alice, AliceAvatarUrl, logger))
            {
                await context.SaveChangesAsync(ct);
            }

            logger.LogDebug("alice already exists");
            return;
        }

        alice = new ApplicationUser
        {
            Id             = AliceId,
            UserName       = "alice",
            Email          = "aliceSmith@gmail.com",
            EmailConfirmed = true,
            FullName       = "Alice Smith",
            AvatarUrl      = AliceAvatarUrl,
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
            throw new InvalidFieldException(UserDomainErrorCode.UserCreationFailed, nameof(ApplicationUser));
        }

        var aliceHomeAddress = new Address(
            fullName: "Alice Smith", phoneNumber: "+84901234567",
            street: "123 Main Street", commune: "Downtown",
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
            new Claim(JwtClaimTypes.Picture,    AliceAvatarUrl),
        ]);
        if (!result.Succeeded)
        {
            logger.LogError("Failed to add claims to alice: {Error}", result.Errors.First().Description);
            throw new InvalidFieldException(UserDomainErrorCode.UserCreationFailed, nameof(ApplicationUser));
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
            if (await EnsureAvatarUrlAsync(userMgr, bob, BobAvatarUrl, logger))
            {
                await context.SaveChangesAsync(ct);
            }

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
            AvatarUrl      = BobAvatarUrl,
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
            throw new InvalidFieldException(UserDomainErrorCode.UserCreationFailed, nameof(ApplicationUser));
        }

        var bobHomeAddress = new Address(
            fullName: "Bob Smith", phoneNumber: "+84987654321",
            street: "456 Oak Avenue", commune: "Suburb",
            province: "Texas", country: "USA",
            zipCode: "67890", addressType: AddressType.Home);

        var bobWorkAddress = new Address(
            fullName: "Bob Smith", phoneNumber: "+84987654321",
            street: "789 Business Blvd", commune: "Business commune",
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
            new Claim(JwtClaimTypes.Picture,    BobAvatarUrl),
        ]);
        if (!result.Succeeded)
        {
            logger.LogError("Failed to create bob: {Error}", result.Errors.First().Description);
            throw new InvalidFieldException(UserDomainErrorCode.UserCreationFailed, nameof(ApplicationUser));
        }

        await context.SaveChangesAsync(ct);
        logger.LogDebug("bob created with sample addresses");
    }

    private static async Task SeedSystemAdminAsync(
        UserManager<ApplicationUser> userMgr, UserDbContext context, ILogger logger, CancellationToken ct)
    {
        var systemAdmin = await userMgr.FindByNameAsync("sysadmin");
        if (systemAdmin != null)
        {
            if (await EnsureAvatarUrlAsync(userMgr, systemAdmin, SystemAdminAvatarUrl, logger))
            {
                await context.SaveChangesAsync(ct);
            }
            logger.LogDebug("sysadmin already exists");
            return;
        }

        systemAdmin = new ApplicationUser
        {
            UserName       = "sysadmin",
            Email          = "sysadmin@hivespace.com",
            EmailConfirmed = true,
            FullName       = "System Administrator",
            AvatarUrl      = SystemAdminAvatarUrl,
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
            throw new InvalidFieldException(UserDomainErrorCode.UserCreationFailed, nameof(ApplicationUser));
        }

        var claimResult = await userMgr.AddClaimsAsync(systemAdmin,
        [
            new Claim(JwtClaimTypes.Name,       "System Administrator"),
            new Claim(JwtClaimTypes.GivenName,  "System"),
            new Claim(JwtClaimTypes.FamilyName, "Administrator"),
            new Claim(JwtClaimTypes.Role,       "SystemAdmin"),
            new Claim("permissions",            "system.manage"),
            new Claim("permissions",            "user.manage"),
            new Claim("permissions",            "store.manage"),
        ]);
        if (!claimResult.Succeeded)
        {
            logger.LogError("Failed to add claims to sysadmin: {Error}", claimResult.Errors.First().Description);
            throw new InvalidFieldException(UserDomainErrorCode.UserCreationFailed, nameof(ApplicationUser));
        }

        logger.LogDebug("sysadmin created");
    }

    private static async Task SeedAdminAsync(
        UserManager<ApplicationUser> userMgr, UserDbContext context,
        ILogger logger, CancellationToken ct)
    {
        var admin = await userMgr.FindByNameAsync("admin");
        if (admin != null)
        {
            if (await EnsureAvatarUrlAsync(userMgr, admin, AdminAvatarUrl, logger))
            {
                await context.SaveChangesAsync(ct);
            }

            logger.LogDebug("admin already exists");
            return;
        }

        admin = new ApplicationUser
        {
            UserName       = "admin",
            Email          = "admin@hivespace.com",
            EmailConfirmed = true,
            FullName       = "Admin User",
            AvatarUrl      = AdminAvatarUrl,
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
            throw new InvalidFieldException(UserDomainErrorCode.UserCreationFailed, nameof(ApplicationUser));
        }

        var adminAddress = new Address(
            fullName: "Admin User", phoneNumber: "+84922222222",
            street: "100 Admin Plaza", commune: "Central",
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
                AvatarUrl    = TikiAvatarUrl,
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
                AvatarUrl    = GiverAvatarUrl,
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
                AvatarUrl    = PhuongDongAvatarUrl,
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
            var seller = await userMgr.Users.FirstOrDefaultAsync(u => u.Id == seed.SellerId, ct)
                      ?? await userMgr.Users.FirstOrDefaultAsync(u => u.UserName == seed.Username, ct);

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
                    EmailConfirmed = true,
                    FullName       = seed.FullName,
                    AvatarUrl      = seed.AvatarUrl,
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
                    throw new InvalidFieldException(UserDomainErrorCode.UserCreationFailed, nameof(ApplicationUser));
                }
            }
            else
            {
                await EnsureAvatarUrlAsync(userMgr, seller, seed.AvatarUrl, logger);
            }

            var store = await context.Stores.FirstOrDefaultAsync(s => s.Id == seed.StoreId, ct)
                     ?? await context.Stores.FirstOrDefaultAsync(s => s.OwnerId == seed.SellerId, ct);

            if (store is null)
            {
                var registration = await storeManager.RegisterStoreAsync(
                    name:         seed.StoreName,
                    description:  seed.StoreDescription,
                    logoUrl:      seed.LogoUrl,
                    storeAddress: seed.StoreAddress,
                    ownerId:      seed.SellerId,
                    storeId:      seed.StoreId);

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

            if (seller.StoreId != seed.StoreId)
            {
                seller.StoreId = seed.StoreId;
                seller.RoleName = Role.RoleNames.Seller;
                var updateResult = await userMgr.UpdateAsync(seller);
                if (!updateResult.Succeeded)
                {
                    logger.LogError("Failed to update seller {Username} with StoreId: {Error}",
                        seed.Username, updateResult.Errors.First().Description);
                    throw new Exception(updateResult.Errors.First().Description);
                }
            }

            var hasAddress = await context.Addresses.AnyAsync(a => EF.Property<Guid>(a, "UserId") == seed.SellerId, ct);
            if (!hasAddress)
            {
                var sellerAddress = new Address(
                    fullName: seed.FullName, phoneNumber: seed.Phone,
                    street: seed.StoreName, commune: "District 1",
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
                    throw new InvalidFieldException(UserDomainErrorCode.UserCreationFailed, nameof(ApplicationUser));
                }
            }

            logger.LogDebug("Seller seed ensured for {Username} (SellerId={SellerId}, StoreId={StoreId}).",
                seed.Username, seed.SellerId, seed.StoreId);
        }
    }

    private static async Task<bool> EnsureAvatarUrlAsync(
        UserManager<ApplicationUser> userMgr,
        ApplicationUser user,
        string avatarUrl,
        ILogger logger)
    {
        if (string.Equals(user.AvatarUrl, avatarUrl, StringComparison.Ordinal))
        {
            return false;
        }

        user.AvatarUrl = avatarUrl;
        var updateResult = await userMgr.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            logger.LogError("Failed to update AvatarUrl for {Username}: {Error}",
                user.UserName, updateResult.Errors.First().Description);
            throw new InvalidFieldException(UserDomainErrorCode.UserCreationFailed, nameof(ApplicationUser));
        }

        return true;
    }
}
