using Microsoft.Extensions.DependencyInjection;

namespace HiveSpace.IdentityService.Core.Extensions;

public static class CoreServicesExtensions
{
    public static IServiceCollection AddCoreServices(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CoreServicesExtensions).Assembly));
        return services;
    }
}
