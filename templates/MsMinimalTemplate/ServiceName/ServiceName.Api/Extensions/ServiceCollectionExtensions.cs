using HiveSpace.Core.Filters;
using ServiceName.Core.Extensions;

namespace ServiceName.Api.Extensions;

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAppApiControllers(this IServiceCollection services)
    {
        services.AddControllers(options => options.Filters.Add<CustomExceptionFilter>());
        return services;
    }

    public static IServiceCollection AddAppAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthentication("Bearer")
            .AddJwtBearer(options =>
            {
                options.Authority = configuration["Identity:Authority"];
                options.Audience = configuration["Identity:Audience"];
            });
        return services;
    }

    public static IServiceCollection AddAppServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddCoreServices();
        services.AddCoreDatabase(configuration);
        return services;
    }
}
