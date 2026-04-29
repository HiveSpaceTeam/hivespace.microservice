using HiveSpace.MediaService.Core.Infrastructure.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace HiveSpace.MediaService.Core.Infrastructure;

public static class DataSeeder
{
    public static async Task EnsureSeedDataAsync(WebApplication app, CancellationToken cancellationToken = default)
    {
        await using var scope = app.Services.CreateAsyncScope();
        var db     = scope.ServiceProvider.GetRequiredService<MediaDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<MediaDbContext>>();

        var pending = (await db.Database.GetPendingMigrationsAsync(cancellationToken)).ToList();
        if (pending.Count > 0)
        {
            logger.LogInformation("Applying {Count} pending migration(s): {Migrations}",
                pending.Count, string.Join(", ", pending));
            await db.Database.MigrateAsync(cancellationToken);
            logger.LogInformation("Migrations applied successfully.");
        }
    }
}
