using HiveSpace.Infrastructure.Persistence.Seeding;
using HiveSpace.NotificationService.Core.SeedData;
using Microsoft.Extensions.DependencyInjection;

namespace HiveSpace.NotificationService.Core.Extensions;

public static class CoreServicesExtensions
{
    public static IServiceCollection AddNotificationSeedData(this IServiceCollection services)
    {
        services.AddScoped<ISeeder, NotificationTemplateSeeder>();
        services.AddScoped<ISeeder, UserRefSeeder>();
        services.AddScoped<ISeeder, UserPreferenceSeeder>();
        return services;
    }
}
