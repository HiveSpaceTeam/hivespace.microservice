using FluentValidation;
using HiveSpace.Application.Shared.Behaviors;
using HiveSpace.UserService.Application.Users.Commands.UpdateUserProfile;
using Microsoft.Extensions.DependencyInjection;

namespace HiveSpace.UserService.Application;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<UpdateUserProfileCommand>();
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblyContaining<UpdateUserProfileCommand>();
            cfg.AddOpenBehavior(typeof(ValidationPipelineBehavior<,>));
        });

        return services;
    }
}