using HiveSpace.IdentityService.Core.DomainModels;
using HiveSpace.IdentityService.Core.Persistence;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace HiveSpace.IdentityService.Core.Infrastructure;

public static partial class DataSeeder
{
    public static readonly Guid AliceId = new Guid("11111111-1111-1111-1111-111111111111");
    public static readonly Guid BobId   = new Guid("22222222-2222-2222-2222-222222222222");
    public static readonly Guid SysAdminId = new Guid("33333333-3333-3333-3333-333333333333");
    public static readonly Guid AdminId = new Guid("44444444-4444-4444-4444-444444444444");

    public static async Task EnsureSeedDataAsync(WebApplication app, CancellationToken ct = default)
    {
        await using var scope = app.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
        var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleMgr = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        var logger  = scope.ServiceProvider.GetRequiredService<ILogger<IdentityDbContext>>();

        var pending = (await context.Database.GetPendingMigrationsAsync(ct)).ToList();
        if (pending.Count > 0)
        {
            logger.LogInformation("Applying {Count} pending migration(s): {Migrations}",
                pending.Count, string.Join(", ", pending));
            await context.Database.MigrateAsync(ct);
            logger.LogInformation("Migrations applied successfully.");
        }

        await SeedRolesAsync(roleMgr, logger, ct);
        await SeedUsersAsync(userMgr, logger, ct);
    }
}
