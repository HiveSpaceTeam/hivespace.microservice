using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace HiveSpace.Infrastructure.Persistence.Outbox;

/// <summary>
/// Extension methods for registering outbox services.
/// </summary>
public static class OutboxServiceCollectionExtensions
{
    public static IServiceCollection AddOutboxServices<TContext>(this IServiceCollection services)
        where TContext : DbContext
    {
        services.AddScoped<IOutboxRepository, OutboxRepository<TContext>>();
        
        // Add background service for processing outbox messages
        // services.AddHostedService<OutboxMessageProcessor>();
        
        return services;
    }
}