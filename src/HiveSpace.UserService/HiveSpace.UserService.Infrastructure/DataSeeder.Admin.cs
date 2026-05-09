using Duende.IdentityModel;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.UserService.Domain.Aggregates.User;
using HiveSpace.UserService.Domain.Enums;
using HiveSpace.UserService.Domain.Exceptions;
using HiveSpace.UserService.Infrastructure.Data;
using HiveSpace.UserService.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace HiveSpace.UserService.Infrastructure;

public static partial class DataSeeder
{
    private const string SystemAdminAvatarUrl = "https://images.unsplash.com/photo-1500648767791-00dcc994a43e?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&ixlib=rb-4.0.3&q=80&w=1080";
    private const string AdminAvatarUrl       = "https://images.unsplash.com/photo-1494790108377-be9c29b29330?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&ixlib=rb-4.0.3&q=80&w=1080";

    private static async Task SeedSystemAdminAsync(
        UserManager<ApplicationUser> userMgr, UserDbContext context,
        ILogger logger, CancellationToken ct)
    {
        var systemAdmin = await userMgr.FindByNameAsync("sysadmin");
        if (systemAdmin != null)
        {
            if (await EnsureAvatarUrlAsync(userMgr, systemAdmin, SystemAdminAvatarUrl, logger))
                await context.SaveChangesAsync(ct);

            logger.LogDebug("sysadmin already exists");
            return;
        }

        systemAdmin = new ApplicationUser
        {
            UserName       = "sysadmin",
            Email          = "sysadmin@hivespace.com",
            EmailConfirmed = true,
            FullName       = "System Administrator",
            AvatarFileId   = Guid.NewGuid().ToString(),
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
                await context.SaveChangesAsync(ct);

            logger.LogDebug("admin already exists");
            return;
        }

        admin = new ApplicationUser
        {
            UserName       = "admin",
            Email          = "admin@hivespace.com",
            EmailConfirmed = true,
            FullName       = "Admin User",
            AvatarFileId   = Guid.NewGuid().ToString(),
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
}
