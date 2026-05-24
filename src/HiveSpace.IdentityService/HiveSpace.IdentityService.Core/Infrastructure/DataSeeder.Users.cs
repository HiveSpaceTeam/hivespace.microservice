using System.Security.Claims;
using Duende.IdentityModel;
using HiveSpace.IdentityService.Core.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace HiveSpace.IdentityService.Core.Infrastructure;

public static partial class DataSeeder
{
    private const string SystemAdminAvatarUrl = "https://images.unsplash.com/photo-1500648767791-00dcc994a43e?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&ixlib=rb-4.0.3&q=80&w=1080";
    private const string AdminAvatarUrl       = "https://images.unsplash.com/photo-1494790108377-be9c29b29330?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&ixlib=rb-4.0.3&q=80&w=1080";
    private const string AliceAvatarUrl       = "https://cdn.discordapp.com/avatars/474579515188707339/24acc2c6b645216504447360a58c0683.webp";
    private const string BobAvatarUrl         = "https://cdn.discordapp.com/avatars/743061397037908059/edaa33d7f618405017a26cc7ca42379b.webp";

    private static async Task SeedRolesAsync(RoleManager<IdentityRole<Guid>> roleMgr, ILogger logger, CancellationToken ct)
    {
        var roles = new[] { "SystemAdmin", "Admin", "Seller", "Buyer" };
        foreach (var roleName in roles)
        {
            if (!await roleMgr.RoleExistsAsync(roleName))
            {
                await roleMgr.CreateAsync(new IdentityRole<Guid> { Name = roleName });
                logger.LogDebug("Role {Role} created", roleName);
            }
        }
    }

    private static async Task SeedUsersAsync(UserManager<ApplicationUser> userMgr, ILogger logger, CancellationToken ct)
    {
        await SeedSystemAdminAsync(userMgr, logger);
        await SeedAdminAsync(userMgr, logger);
        await SeedAliceAsync(userMgr, logger);
        await SeedBobAsync(userMgr, logger);
        await SeedSellersAsync(userMgr, logger);
    }

    private static async Task SeedSystemAdminAsync(UserManager<ApplicationUser> userMgr, ILogger logger)
    {
        var sysadmin = await userMgr.FindByNameAsync("sysadmin");
        if (sysadmin != null)
        {
            await EnsureUserRoleAsync(userMgr, sysadmin, "SystemAdmin", logger);
            return;
        }

        sysadmin = new ApplicationUser
        {
            Id = SysAdminId,
            UserName = "sysadmin",
            Email = "sysadmin@hivespace.com",
            EmailConfirmed = true,
            PhoneNumber = "+84911111111",
            RoleName = "SystemAdmin",
            Status = 1, // Active
            CreatedAt = DateTimeOffset.UtcNow
        };

        var result = await userMgr.CreateAsync(sysadmin, "SysAdmin123$");
        if (result.Succeeded)
        {
            await EnsureUserRoleAsync(userMgr, sysadmin, "SystemAdmin", logger);
            await userMgr.AddClaimsAsync(sysadmin, new[]
            {
                new Claim(JwtClaimTypes.Name,       "System Administrator"),
                new Claim(JwtClaimTypes.GivenName,  "System"),
                new Claim(JwtClaimTypes.FamilyName, "Administrator"),
                new Claim(JwtClaimTypes.Role,       "SystemAdmin"),
                new Claim(JwtClaimTypes.Picture,    SystemAdminAvatarUrl),
                new Claim("permissions",            "system.manage"),
                new Claim("permissions",            "user.manage"),
                new Claim("permissions",            "store.manage"),
            });
        }
    }

    private static async Task SeedAdminAsync(UserManager<ApplicationUser> userMgr, ILogger logger)
    {
        var admin = await userMgr.FindByNameAsync("admin");
        if (admin != null)
        {
            await EnsureUserRoleAsync(userMgr, admin, "Admin", logger);
            return;
        }

        admin = new ApplicationUser
        {
            Id = AdminId,
            UserName = "admin",
            Email = "admin@hivespace.com",
            EmailConfirmed = true,
            PhoneNumber = "+84922222222",
            RoleName = "Admin",
            Status = 1, // Active
            CreatedAt = DateTimeOffset.UtcNow
        };

        var result = await userMgr.CreateAsync(admin, "Admin123$");
        if (result.Succeeded)
        {
            await EnsureUserRoleAsync(userMgr, admin, "Admin", logger);
            await userMgr.AddClaimsAsync(admin, new[]
            {
                new Claim(JwtClaimTypes.Name,       "Admin User"),
                new Claim(JwtClaimTypes.GivenName,  "Admin"),
                new Claim(JwtClaimTypes.FamilyName, "User"),
                new Claim(JwtClaimTypes.Role,       "Admin"),
                new Claim(JwtClaimTypes.Picture,    AdminAvatarUrl),
                new Claim("permissions",            "user.manage"),
                new Claim("permissions",            "store.view"),
            });
        }
    }

    private static async Task SeedAliceAsync(UserManager<ApplicationUser> userMgr, ILogger logger)
    {
        var alice = await userMgr.FindByNameAsync("alice");
        if (alice != null) return;

        alice = new ApplicationUser
        {
            Id = AliceId,
            UserName = "alice",
            Email = "aliceSmith@gmail.com",
            EmailConfirmed = true,
            PhoneNumber = "+84901234567",
            Status = 1, // Active
            CreatedAt = DateTimeOffset.UtcNow
        };

        var result = await userMgr.CreateAsync(alice, "AliceSmith123$");
        if (result.Succeeded)
        {
            await userMgr.AddClaimsAsync(alice, new[]
            {
                new Claim(JwtClaimTypes.Name,       "Alice Smith"),
                new Claim(JwtClaimTypes.GivenName,  "Alice"),
                new Claim(JwtClaimTypes.FamilyName, "Smith"),
                new Claim(JwtClaimTypes.WebSite,    "http://alice.example.com"),
                new Claim(JwtClaimTypes.Picture,    AliceAvatarUrl),
            });
        }
    }

    private static async Task SeedBobAsync(UserManager<ApplicationUser> userMgr, ILogger logger)
    {
        var bob = await userMgr.FindByNameAsync("bob");
        if (bob != null) return;

        bob = new ApplicationUser
        {
            Id = BobId,
            UserName = "bob",
            Email = "bobSmith@gmail.com",
            EmailConfirmed = false,
            PhoneNumber = "+84987654321",
            Status = 1, // Active
            CreatedAt = DateTimeOffset.UtcNow
        };

        var result = await userMgr.CreateAsync(bob, "BobSmith123$");
        if (result.Succeeded)
        {
            await userMgr.AddClaimsAsync(bob, new[]
            {
                new Claim(JwtClaimTypes.Name,       "Bob Smith"),
                new Claim(JwtClaimTypes.GivenName,  "Bob"),
                new Claim(JwtClaimTypes.FamilyName, "Smith"),
                new Claim(JwtClaimTypes.WebSite,    "http://bob.example.com"),
                new Claim("location",               "somewhere"),
                new Claim(JwtClaimTypes.Picture,    BobAvatarUrl),
            });
        }
    }

    private static async Task SeedSellersAsync(UserManager<ApplicationUser> userMgr, ILogger logger)
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
                Password         = "TikiTrading123$",
                GivenName        = "Tiki",
                FamilyName       = "Trading"
            },
            new
            {
                SellerId         = new Guid("c3d4e5f6-a7b8-9012-cdef-012345678901"),
                StoreId          = new Guid("e5f6a7b8-c9d0-1234-ef01-234567890123"),
                Username         = "giver",
                Email            = "giver@gmail.com",
                FullName         = "GIVER BOOKS & MEDIA",
                Phone            = "+84901000002",
                Password         = "GiverBooks123$",
                GivenName        = "Giver",
                FamilyName       = "Books",
            },
            new
            {
                SellerId         = new Guid("d4e5f6a7-b8c9-0123-def0-123456789012"),
                StoreId          = new Guid("f6a7b8c9-d0e1-2345-f012-345678901234"),
                Username         = "phuongdong",
                Email            = "phuongdong@gmail.com",
                FullName         = "Phương Đông Books",
                Phone            = "+84901000003",
                Password         = "PhuongDongBooks123$",
                GivenName        = "Phuong",
                FamilyName       = "Dong"
            },
        };

        foreach (var seed in sellerSeeds)
        {
            var seller = await userMgr.FindByNameAsync(seed.Username);
            if (seller != null)
            {
                await EnsureUserRoleAsync(userMgr, seller, "Seller", logger);
                continue;
            }

            seller = new ApplicationUser
            {
                Id             = seed.SellerId,
                UserName       = seed.Username,
                Email          = seed.Email,
                EmailConfirmed = true,
                PhoneNumber    = seed.Phone,
                Status         = 1, // Active
                RoleName       = "Seller",
                StoreId        = seed.StoreId,
                CreatedAt      = DateTimeOffset.UtcNow
            };

            var result = await userMgr.CreateAsync(seller, seed.Password);
            if (result.Succeeded)
            {
                await EnsureUserRoleAsync(userMgr, seller, "Seller", logger);
                await userMgr.AddClaimsAsync(seller, new[]
                {
                    new Claim(JwtClaimTypes.Name,       seed.FullName),
                    new Claim(JwtClaimTypes.GivenName,  seed.GivenName),
                    new Claim(JwtClaimTypes.FamilyName, seed.FamilyName),
                    new Claim(JwtClaimTypes.Role,       "Seller"),
                    new Claim("permissions",            "store.manage"),
                    new Claim("permissions",            "product.manage"),
                    new Claim("permissions",            "order.view"),
                });
            }
        }
    }

    private static async Task EnsureUserRoleAsync(
        UserManager<ApplicationUser> userMgr,
        ApplicationUser user,
        string role,
        ILogger logger)
    {
        if (!await userMgr.IsInRoleAsync(user, role))
        {
            var result = await userMgr.AddToRoleAsync(user, role);
            if (!result.Succeeded)
            {
                var errors = string.Join("; ", result.Errors.Select(error => error.Description));
                logger.LogWarning("Failed to add seeded user {UserName} to role {Role}: {Errors}", user.UserName, role, errors);
            }
        }
    }
}
