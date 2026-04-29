using FluentValidation;
using HiveSpace.Application.Shared.Behaviors;
using HiveSpace.OrderService.Application.Coupons.Commands.CreateCoupon;
using Microsoft.Extensions.DependencyInjection;

namespace HiveSpace.OrderService.Application;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<CreateCouponCommand>();
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblyContaining<CreateCouponCommand>();
            cfg.AddOpenBehavior(typeof(ValidationPipelineBehavior<,>));
        });
        return services;
    }
}
