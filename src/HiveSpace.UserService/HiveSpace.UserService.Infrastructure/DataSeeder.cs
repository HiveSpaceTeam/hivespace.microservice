using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.UserService.Domain.Exceptions;
using HiveSpace.UserService.Domain.Services;
using HiveSpace.UserService.Infrastructure.Data;
using HiveSpace.UserService.Infrastructure.Identity;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace HiveSpace.UserService.Infrastructure;

public static partial class DataSeeder
{
    public static readonly Guid AliceId = new Guid("11111111-1111-1111-1111-111111111111");
    public static readonly Guid BobId   = new Guid("22222222-2222-2222-2222-222222222222");

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

    private static async Task<bool> EnsureAvatarUrlAsync(
        UserManager<ApplicationUser> userMgr,
        ApplicationUser user,
        string avatarUrl,
        ILogger logger)
    {
        if (string.Equals(user.AvatarUrl, avatarUrl, StringComparison.Ordinal))
            return false;

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
