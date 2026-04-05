using HiveSpace.Domain.Shared.IdGeneration;
using HiveSpace.Infrastructure.Persistence.Seeding;
using HiveSpace.OrderService.Infrastructure.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace HiveSpace.OrderService.Infrastructure;

public static class DataSeeder
{
    public static async Task EnsureSeedDataAsync(WebApplication app, CancellationToken cancellationToken = default)
    {
        await using var scope = app.Services.CreateAsyncScope();
        var db     = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<OrderDbContext>>();

        // Pre-init so domain entities can be created during seeding (first-caller-wins, idempotent)
        var guidGen = scope.ServiceProvider.GetRequiredService<IIdGenerator<Guid>>();
        var longGen = scope.ServiceProvider.GetRequiredService<IIdGenerator<long>>();
        IdGenerator.Initialize(guidGen, longGen);

        var pending = (await db.Database.GetPendingMigrationsAsync(cancellationToken)).ToList();
        if (pending.Count > 0)
        {
            logger.LogInformation("Applying {Count} pending migration(s): {Migrations}",
                pending.Count, string.Join(", ", pending));
            await db.Database.MigrateAsync(cancellationToken);
            logger.LogInformation("Migrations applied successfully.");
        }

        var seeders = scope.ServiceProvider
            .GetRequiredService<IEnumerable<ISeeder>>()
            .OrderBy(s => s.Order);

        foreach (var seeder in seeders)
            await seeder.SeedAsync(cancellationToken);
    }
}
