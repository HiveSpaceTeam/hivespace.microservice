using HiveSpace.CatalogService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace HiveSpace.CatalogService.Infrastructure;

using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class SeedData
{
    public static async Task EnsureSeedDataAsync(WebApplication app, CancellationToken cancellationToken = default)
    {
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();

        var pending = (await context.Database.GetPendingMigrationsAsync(cancellationToken)).ToList();
        if (pending.Count > 0)
        {
            Log.Information("Found {Count} pending migrations: {Migrations}", pending.Count, string.Join(", ", pending));
            Log.Information("Applying pending migrations...");
            await context.Database.MigrateAsync(cancellationToken);
            Log.Information("Migrations applied successfully");
        }
        else
        {
            Log.Information("No pending migrations found. Database is up to date.");
        }
    }
}
