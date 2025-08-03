using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace HiveSpace.IdentityService.Application.Extensions;

public static class MediatRRegistration
{
    public static IServiceCollection AddMediatRRegistration(this IServiceCollection services, IConfiguration configuration)
    {
        // Register MediatR for both API and Application layers
        services.AddMediatR(cfg =>
        {
            cfg.LicenseKey = configuration.GetValue("MediatR:LicenseKey", "");
            cfg.RegisterServicesFromAssemblyContaining(typeof(MediatRRegistration));
        });
        return services;
    }
}
