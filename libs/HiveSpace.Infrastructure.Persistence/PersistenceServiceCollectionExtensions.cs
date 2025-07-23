using Microsoft.Extensions.DependencyInjection;
using HiveSpace.Infrastructure.Persistence.Idempotence;
using HiveSpace.Infrastructure.Persistence.Outbox;
using HiveSpace.Infrastructure.Persistence.Transaction;
using Microsoft.EntityFrameworkCore;

namespace HiveSpace.Infrastructure.Persistence;

/// <summary>
/// Extension methods for registering persistence services.
/// </summary>
public static class PersistenceServiceCollectionExtensions
{
    /// <summary>
    /// Adds all persistence services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddPersistenceInfrastructure(this IServiceCollection services)
    {
        // Add idempotence services
        services.AddScoped<IIncomingRequestRepository, IncomingRequestRepository>();
        
        // Add transaction services
        services.AddScoped<ITransactionService, TransactionService>();
        // Add outbox services
        services.AddOutboxServices();
        
        return services;
    }


    public static ModelBuilder AddPersistenceBuilder(this ModelBuilder builder)
    {
        builder.ApplyConfiguration(new OutboxMessageEntityConfiguration());
        builder.ApplyConfiguration(new IncomingRequestEntityConfiguration());
        return builder;
    }
} 