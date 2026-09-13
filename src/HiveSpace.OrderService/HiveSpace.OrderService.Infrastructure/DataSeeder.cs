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
    public static async Task InitializeAsync(
        WebApplication app,
        bool autoMigrate,
        bool seedSampleData,
        CancellationToken cancellationToken = default)
    {
        await using var scope = app.Services.CreateAsyncScope();
        var db     = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<OrderDbContext>>();

        // Pre-init so domain entities can be created during seeding (first-caller-wins, idempotent)
        var guidGen = scope.ServiceProvider.GetRequiredService<IIdGenerator<Guid>>();
        var longGen = scope.ServiceProvider.GetRequiredService<IIdGenerator<long>>();
        IdGenerator.Initialize(guidGen, longGen);

        var seeders = scope.ServiceProvider
            .GetRequiredService<IEnumerable<ISeeder>>()
            .OrderBy(s => s.Order);

        if (autoMigrate)
        {
            var pending = (await db.Database.GetPendingMigrationsAsync(cancellationToken)).ToList();
            if (pending.Count > 0)
            {
                logger.LogInformation("Applying {Count} pending migration(s): {Migrations}",
                    pending.Count, string.Join(", ", pending));
                await db.Database.MigrateAsync(cancellationToken);
                logger.LogInformation("Migrations applied successfully.");
            }
        }
        else
        {
            logger.LogInformation("Automatic migration disabled for OrderService.");
        }

        foreach (var seeder in seeders.Where(s => s.Kind == SeedKind.ReferenceData))
            await seeder.SeedAsync(cancellationToken);

        foreach (var seeder in seeders.Where(s => s.Kind == SeedKind.BootstrapData))
            await seeder.SeedAsync(cancellationToken);

        if (!seedSampleData)
        {
            logger.LogInformation("Sample data seeding disabled for OrderService.");
            return;
        }

        foreach (var seeder in seeders.Where(s => s.Kind == SeedKind.SampleData))
            await seeder.SeedAsync(cancellationToken);
    }
}
