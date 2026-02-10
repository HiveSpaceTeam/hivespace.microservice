using Microsoft.Extensions.DependencyInjection;
using HiveSpace.Infrastructure.Persistence.Idempotence;
using HiveSpace.Infrastructure.Persistence.Outbox;
using HiveSpace.Infrastructure.Persistence.Transaction;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using HiveSpace.Infrastructure.Persistence.Interceptors;

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
        // Register generic repositories and services
        services.AddScoped(typeof(IncomingRequestRepository<>));
        services.AddScoped(typeof(TransactionService<>));
        
        return services;
    }

    /// <summary>
    /// Adds persistence services with a specific DbContext type.
    /// </summary>
    /// <typeparam name="TContext">The DbContext type</typeparam>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddPersistenceInfrastructure<TContext>(this IServiceCollection services)
        where TContext : DbContext
    {
        // Add idempotence services
        services.AddScoped<IIncomingRequestRepository, IncomingRequestRepository<TContext>>();
        
        // Add transaction services
        services.AddScoped<ITransactionService, TransactionService<TContext>>();
        
        return services;
    }

    public static ModelBuilder AddPersistenceBuilder(this ModelBuilder builder)
    {
        builder.ApplyConfiguration(new IncomingRequestEntityConfiguration());
        return builder;
    }

    public static IServiceCollection AddAppInterceptors(this IServiceCollection services)
    {
        // Register all interceptors from the current assembly
        services.AddScoped<ISaveChangesInterceptor, AuditableInterceptor>();
        services.AddScoped<ISaveChangesInterceptor, SoftDeleteInterceptor>();
        //services.AddScoped<ISaveChangesInterceptor, DomainEventToOutboxInterceptor>();
        services.AddScoped<ISaveChangesInterceptor, DispatchDomainEventInterceptor>();

        return services;
    }
}