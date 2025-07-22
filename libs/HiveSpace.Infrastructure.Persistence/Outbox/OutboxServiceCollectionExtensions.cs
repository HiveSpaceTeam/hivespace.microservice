using Microsoft.Extensions.DependencyInjection;

namespace HiveSpace.Infrastructure.Persistence.Outbox;

/// <summary>
/// Extension methods for registering outbox services.
/// </summary>
public static class OutboxServiceCollectionExtensions
{
    /// <summary>
    /// Adds outbox services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddOutboxServices(this IServiceCollection services)
    {
        services.AddScoped<IOutboxRepository, OutboxRepository>();
        services.AddHostedService<OutboxMessageProcessor>();
        
        return services;
    }
} 