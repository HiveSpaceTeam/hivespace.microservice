using HiveSpace.CatalogService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

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

        var logger = scope.ServiceProvider.GetRequiredService<Microsoft.Extensions.Logging.ILogger<SeedData>>();

        Console.WriteLine("Checking for pending migrations...");
        var pending = (await context.Database.GetPendingMigrationsAsync(cancellationToken)).ToList();
        if (pending.Count > 0)
        {
            logger.LogInformation("Found {Count} pending migrations: {Migrations}", pending.Count, string.Join(", ", pending));
            Console.WriteLine($"Found {pending.Count} pending migrations. Applying...");
            
            await context.Database.MigrateAsync(cancellationToken);
            
            logger.LogInformation("Migrations applied successfully");
            Console.WriteLine("Migrations applied successfully.");
        }
        else
        {
            logger.LogInformation("No pending migrations found. Database is up to date.");
            Console.WriteLine("No pending migrations found. Database is up to date.");
        }
    }
}
