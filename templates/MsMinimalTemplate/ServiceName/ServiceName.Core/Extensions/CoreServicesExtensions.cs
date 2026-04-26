using Microsoft.EntityFrameworkCore;
using ServiceName.Core.Persistence;

namespace ServiceName.Core.Extensions;

public static class CoreServicesExtensions
{
    public static IServiceCollection AddCoreServices(this IServiceCollection services)
    {
        // TODO: Register services and repositories here (all AddScoped)
        // services.AddScoped<I[Feature]Service, [Feature]Service>();
        // services.AddScoped<I[Entity]Repository, [Entity]Repository>();
        return services;
    }

    public static IServiceCollection AddCoreDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ServiceNameDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
        return services;
    }
}
