using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using HiveSpace.Core.Contexts;
using HiveSpace.Core.Filters;
using HiveSpace.Core.IdGeneration;
using HiveSpace.Domain.Shared.IdGeneration;

namespace HiveSpace.Core;

/// <summary>
/// Extension methods for registering core services.
/// </summary>
public static class CoreServiceCollectionExtensions
{
    /// <summary>
    /// Adds all core services to the service collection.
    /// </summary>
    public static IServiceCollection AddCoreServices(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<IRequestContext, RequestContext>();
        services.AddScoped<IUserContext, UserContext>();
        services.AddScoped<CustomExceptionFilter>();

        return services;
    }

    /// <summary>
    /// Registers ID generators (Snowflake for <see cref="long"/>, UUID v7 for <see cref="Guid"/>)
    /// and initializes the <see cref="IdGenerator"/> static gateway for use in domain entities.
    /// Reads Snowflake settings from the <c>Snowflake</c> configuration section.
    /// </summary>
    public static IServiceCollection AddIdGenerators(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<SnowflakeOptions>(
            configuration.GetSection(SnowflakeOptions.SectionName));

        services.AddSingleton<IIdGenerator<long>, SnowflakeIdGenerator>();
        services.AddSingleton<IIdGenerator<Guid>, GuidV7Generator>();
        services.AddHostedService<IdGeneratorInitializer>();

        return services;
    }
}
