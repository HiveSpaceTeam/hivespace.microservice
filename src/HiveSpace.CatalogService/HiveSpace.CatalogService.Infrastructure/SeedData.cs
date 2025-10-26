using HiveSpace.CatalogService.Infrastructure.Data;
using HiveSpace.CatalogService.Domain.Aggregates.ProductAggregate;
using HiveSpace.CatalogService.Domain.Aggregates.CategoryAggregate;
using HiveSpace.CatalogService.Domain.Aggregates.AttributeAggregate;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace HiveSpace.CatalogService.Infrastructure;

public class SeedData
{
    public static void EnsureSeedData(WebApplication app)
    {
        using (var scope = app.Services.GetRequiredService<IServiceScopeFactory>().CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
            
            // Check for pending migrations before applying them
            var pendingMigrations = context.Database.GetPendingMigrations();
            if (pendingMigrations.Any())
            {
                Log.Information("Found {Count} pending migrations: {Migrations}", 
                    pendingMigrations.Count(), 
                    string.Join(", ", pendingMigrations));
                
                Log.Information("Applying pending migrations...");
                context.Database.Migrate();
                Log.Information("Migrations applied successfully");
            }
            else
            {
                Log.Information("No pending migrations found. Database is up to date.");
            }
        }
    }
}