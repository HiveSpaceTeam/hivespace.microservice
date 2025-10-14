using Microsoft.Extensions.DependencyInjection;

namespace HiveSpace.UserService.Application;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Register MediatR handlers from the Application assembly
        services.AddMediatR(cfg => 
        {
            cfg.RegisterServicesFromAssembly(typeof(ApplicationServiceCollectionExtensions).Assembly);
        });

        return services;
    }
}