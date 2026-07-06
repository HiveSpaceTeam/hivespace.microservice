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

    public static async Task EnsureSeedDataAsync(WebApplication app, CancellationToken ct = default)
    {
        await using var scope = app.Services.CreateAsyncScope();
        var context      = scope.ServiceProvider.GetRequiredService<UserDbContext>();
        var storeManager = scope.ServiceProvider.GetRequiredService<StoreManager>();
        var eventPublisher = scope.ServiceProvider.GetRequiredService<IPlatformCurrencyConfigEventPublisher>();
        var logger       = scope.ServiceProvider.GetRequiredService<ILogger<UserDbContext>>();

        var pending = (await context.Database.GetPendingMigrationsAsync(ct)).ToList();
        if (pending.Count > 0)
        {
            logger.LogInformation("Applying {Count} pending migration(s): {Migrations}",
                pending.Count, string.Join(", ", pending));
            await context.Database.MigrateAsync(ct);
            logger.LogInformation("Migrations applied successfully.");
        }

        await SeedAliceAsync(context, logger, ct);
        await SeedBobAsync(context, logger, ct);
        await SeedSystemAdminAsync(context, logger, ct);
        await SeedAdminAsync(context, logger, ct);
        await SeedPlatformCurrencyPolicyAsync(context, eventPublisher, logger, ct);
        await SeedSellersAsync(storeManager, context, logger, ct);
    }

    internal static async Task SeedPlatformCurrencyPolicyAsync(
        UserDbContext context,
        IPlatformCurrencyConfigEventPublisher eventPublisher,
        ILogger logger,
        CancellationToken ct)
    {
        if (await context.PlatformConfigs.AnyAsync(x => x.ConfigType == PlatformConfig.CurrencyConfigType, ct))
            return;

        var config = PlatformConfig.CreateCurrencyPolicy("VND");
        var currencies = new[]
        {
            PlatformCurrency.CreateCurrency("VND", true, 0),
            PlatformCurrency.CreateCurrency("USD", false, 1),
            PlatformCurrency.CreateCurrency("EUR", false, 2)
        };

        context.PlatformConfigs.Add(config);
        context.PlatformCurrencies.AddRange(currencies);

        await eventPublisher.PublishPolicyUpdatedAsync(config, currencies, ct);
        await context.SaveChangesAsync(ct);
        logger.LogInformation("Seeded platform currency policy.");
    }
}
