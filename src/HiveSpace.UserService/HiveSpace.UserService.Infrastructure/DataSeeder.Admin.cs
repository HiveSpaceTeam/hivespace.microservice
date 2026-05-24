using HiveSpace.UserService.Domain.Aggregates.User;
using HiveSpace.Domain.Shared.Enumerations;
using HiveSpace.UserService.Domain.Enums;
using HiveSpace.UserService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HiveSpace.UserService.Infrastructure;

public static partial class DataSeeder
{
    private const string SystemAdminAvatarUrl = "https://images.unsplash.com/photo-1500648767791-00dcc994a43e?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&ixlib=rb-4.0.3&q=80&w=1080";
    private const string AdminAvatarUrl       = "https://images.unsplash.com/photo-1494790108377-be9c29b29330?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&ixlib=rb-4.0.3&q=80&w=1080";

    private static async Task SeedSystemAdminAsync(
        UserDbContext context,
        ILogger logger, CancellationToken ct)
    {
        var exists = await context.Users.AnyAsync(u => u.Id == SysAdminId, ct);
        if (exists)
        {
            logger.LogDebug("sysadmin profile already exists");
            return;
        }

        var systemAdmin = User.CreateProfile(
            id: SysAdminId,
            email: Email.Create("sysadmin@hivespace.com"),
            userName: "sysadmin",
            fullName: "System Administrator",
            avatarUrl: SystemAdminAvatarUrl,
            phoneNumber: PhoneNumber.CreateOrDefault("+84911111111"),
            dateOfBirth: DateOfBirth.CreateOrDefault(new DateTime(1980, 3, 10)),
            gender: Gender.Male,
            createdAt: DateTimeOffset.UtcNow
        );
        systemAdmin.UpdateTheme(Theme.Light);
        systemAdmin.UpdateCulture(Culture.Vi);

        context.Users.Add(systemAdmin);
        await context.SaveChangesAsync(ct);
        
        logger.LogDebug("sysadmin profile created");
    }

    private static async Task SeedAdminAsync(
        UserDbContext context,
        ILogger logger, CancellationToken ct)
    {
        var exists = await context.Users.AnyAsync(u => u.Id == AdminId, ct);
        if (exists)
        {
            logger.LogDebug("admin profile already exists");
            return;
        }

        var admin = User.CreateProfile(
            id: AdminId,
            email: Email.Create("admin@hivespace.com"),
            userName: "admin",
            fullName: "Admin User",
            avatarUrl: AdminAvatarUrl,
            phoneNumber: PhoneNumber.CreateOrDefault("+84922222222"),
            dateOfBirth: DateOfBirth.CreateOrDefault(new DateTime(1985, 8, 22)),
            gender: Gender.Female,
            createdAt: DateTimeOffset.UtcNow
        );
        admin.UpdateTheme(Theme.Light);
        admin.UpdateCulture(Culture.Vi);

        admin.AddAddress(
            fullName: "Admin User", phoneNumber: "+84922222222",
            street: "100 Admin Plaza", commune: "Central",
            province: "New York", country: "USA",
            zipCode: "10001", addressType: AddressType.Work,
            setAsDefault: true);

        context.Users.Add(admin);
        await context.SaveChangesAsync(ct);
        logger.LogDebug("admin profile created with sample address");
    }
}
