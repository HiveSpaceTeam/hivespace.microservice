using HiveSpace.Core.Filters;
using ServiceName.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ServiceName.Api.Extensions;

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAppApiControllers(this IServiceCollection services)
    {
        services.AddControllers(options => options.Filters.Add<CustomExceptionFilter>());
        return services;
    }

    public static IServiceCollection AddAppDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ServiceNameDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
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

    // TODO: Register application services, repositories, and data queries here
    // services.AddScoped<I[Feature]Service, [Feature]Service>();
    // services.AddScoped<I[Root]Repository, Sql[Root]Repository>();
    public static IServiceCollection AddAppApplicationServices(this IServiceCollection services)
    {
        return services;
    }
}
