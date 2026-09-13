using HiveSpace.UserService.Domain.Services;
using HiveSpace.UserService.Domain.Aggregates.Configuration;
using HiveSpace.UserService.Application.Interfaces.Messaging;
using HiveSpace.UserService.Infrastructure.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace HiveSpace.UserService.Infrastructure;

public static partial class DataSeeder
{
    public static readonly Guid AliceId = new Guid("11111111-1111-1111-1111-111111111111");
    public static readonly Guid BobId   = new Guid("22222222-2222-2222-2222-222222222222");
    public static readonly Guid SysAdminId = new Guid("33333333-3333-3333-3333-333333333333");
    public static readonly Guid AdminId = new Guid("44444444-4444-4444-4444-444444444444");

    public static async Task InitializeAsync(
        WebApplication app,
        bool autoMigrate,
        bool seedSampleData,
        CancellationToken ct = default)
    {
        await using var scope = app.Services.CreateAsyncScope();
        var context      = scope.ServiceProvider.GetRequiredService<UserDbContext>();
        var storeManager = scope.ServiceProvider.GetRequiredService<StoreManager>();
        var userEventPublisher = scope.ServiceProvider.GetRequiredService<IUserEventPublisher>();
        var platformCurrencyEventPublisher = scope.ServiceProvider.GetRequiredService<IPlatformCurrencyConfigEventPublisher>();
        var storeEventPublisher = scope.ServiceProvider.GetRequiredService<IStoreEventPublisher>();
        var logger       = scope.ServiceProvider.GetRequiredService<ILogger<UserDbContext>>();

        if (autoMigrate)
        {
            var pending = (await context.Database.GetPendingMigrationsAsync(ct)).ToList();
            if (pending.Count > 0)
            {
                logger.LogInformation("Applying {Count} pending migration(s): {Migrations}",
                    pending.Count, string.Join(", ", pending));
                await context.Database.MigrateAsync(ct);
                logger.LogInformation("Migrations applied successfully.");
            }
        }
        else
        {
            logger.LogInformation("Automatic migration disabled for UserService.");
        }

        await SeedPlatformCurrencyPolicyAsync(context, platformCurrencyEventPublisher, logger, ct);

        if (!seedSampleData)
        {
            logger.LogInformation("Sample data seeding disabled for UserService.");
            return;
        }

        await SeedAliceAsync(context, userEventPublisher, logger, ct);
        await SeedBobAsync(context, userEventPublisher, logger, ct);
        await SeedSystemAdminAsync(context, userEventPublisher, logger, ct);
        await SeedAdminAsync(context, userEventPublisher, logger, ct);
        await SeedSellersAsync(storeManager, context, userEventPublisher, storeEventPublisher, logger, ct);
    }

    internal static async Task SeedPlatformCurrencyPolicyAsync(
        UserDbContext context,
        IPlatformCurrencyConfigEventPublisher eventPublisher,
        ILogger logger,
        CancellationToken ct)
    {
        var config = await context.PlatformConfigs
            .SingleOrDefaultAsync(x => x.ConfigType == PlatformConfig.CurrencyConfigType, ct);
        PlatformCurrency[] currencies;

        if (config is null)
        {
            config = PlatformConfig.CreateCurrencyPolicy("VND");
            currencies =
            [
                PlatformCurrency.CreateCurrency("VND", true, 0),
                PlatformCurrency.CreateCurrency("USD", false, 1),
                PlatformCurrency.CreateCurrency("EUR", false, 2)
            ];

            context.PlatformConfigs.Add(config);
            context.PlatformCurrencies.AddRange(currencies);
            await eventPublisher.PublishPolicyUpdatedAsync(config, currencies, ct);
            await context.SaveChangesAsync(ct);
            logger.LogInformation("Seeded platform currency policy.");
            return;
        }
        else
        {
            currencies = await context.PlatformCurrencies
                .OrderBy(x => x.SortOrder)
                .ToArrayAsync(ct);
            logger.LogDebug("Platform currency policy already exists. Replaying sync event.");
        }

        await eventPublisher.PublishPolicyUpdatedAsync(config, currencies, ct);
        await context.SaveChangesAsync(ct);
    }
}
