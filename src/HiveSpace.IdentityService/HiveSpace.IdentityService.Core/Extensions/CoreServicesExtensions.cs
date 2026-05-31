using FluentValidation;
using HiveSpace.Application.Shared.Behaviors;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace HiveSpace.IdentityService.Core.Extensions;

public static class CoreServicesExtensions
{
    public static IServiceCollection AddCoreServices(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(CoreServicesExtensions).Assembly);
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(CoreServicesExtensions).Assembly);
            cfg.AddOpenBehavior(typeof(ValidationPipelineBehavior<,>));
        });

        return services;
    }
}
