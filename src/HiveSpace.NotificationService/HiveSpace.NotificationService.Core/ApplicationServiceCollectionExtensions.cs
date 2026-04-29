using FluentValidation;
using HiveSpace.Application.Shared.Behaviors;
using HiveSpace.NotificationService.Core.Features.Notifications.Queries.GetNotifications;
using Microsoft.Extensions.DependencyInjection;

namespace HiveSpace.NotificationService.Core;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<GetNotificationsQuery>();
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblyContaining<GetNotificationsQuery>();
            cfg.AddOpenBehavior(typeof(ValidationPipelineBehavior<,>));
        });
        return services;
    }
}
