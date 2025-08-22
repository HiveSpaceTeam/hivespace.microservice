using Microsoft.Extensions.DependencyInjection;

namespace HiveSpace.UserService.Domain.Services;

/// <summary>
/// Extension methods for registering domain services with dependency injection.
/// </summary>
public static class DomainServiceCollectionExtensions
{
    /// <summary>
    /// Registers all domain services with the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddDomainServices(this IServiceCollection services)
    {
        // Register domain services (using Manager pattern)
        services.AddScoped<StoreManager>();
        services.AddScoped<AdminManager>();
        services.AddScoped<UserManager>();
        
        return services;
    }
}
