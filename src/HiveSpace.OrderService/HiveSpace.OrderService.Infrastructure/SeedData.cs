using HiveSpace.OrderService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HiveSpace.OrderService.Infrastructure;

public class SeedData
{
    public static async Task EnsureSeedDataAsync(WebApplication app, CancellationToken cancellationToken = default)
    {
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<OrderDbContext>();

        var logger = scope.ServiceProvider.GetRequiredService<ILogger<SeedData>>();

        System.Console.WriteLine("Checking for pending migrations...");
        var pending = (await context.Database.GetPendingMigrationsAsync(cancellationToken)).ToList();
        if (pending.Count > 0)
        {
            logger.LogInformation("Found {Count} pending migrations: {Migrations}", pending.Count, string.Join(", ", pending));
            System.Console.WriteLine($"Found {pending.Count} pending migrations. Applying...");
            
            await context.Database.MigrateAsync(cancellationToken);
            
            logger.LogInformation("Migrations applied successfully");
            System.Console.WriteLine("Migrations applied successfully.");
        }
        else
        {
            logger.LogInformation("No pending migrations found. Database is up to date.");
            System.Console.WriteLine("No pending migrations found. Database is up to date.");
        }
    }
}
