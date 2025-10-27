using Microsoft.Extensions.DependencyInjection;
using HiveSpace.Core.Contexts;
using HiveSpace.Core.Filters;

namespace HiveSpace.Core;

/// <summary>
/// Extension methods for registering core services.
/// </summary>
public static class CoreServiceCollectionExtensions
{
    /// <summary>
    /// Adds all core services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddCoreServices(this IServiceCollection services)
    {
        // Register HTTP context accessor (required for UserContext)
        services.AddHttpContextAccessor();
        
        // Register contexts
        services.AddScoped<IRequestContext, RequestContext>();
        services.AddScoped<IUserContext, UserContext>();

        // Register filters
        services.AddScoped<CustomExceptionFilter>();

        return services;
    }
}