using FluentValidation;
using HiveSpace.Application.Shared.Behaviors;
using HiveSpace.PaymentService.Application.Payments.Commands.ProcessPaymentWebhook;
using Microsoft.Extensions.DependencyInjection;

namespace HiveSpace.PaymentService.Application;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<ProcessPaymentWebhookCommand>();
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblyContaining<ProcessPaymentWebhookCommand>();
            cfg.AddOpenBehavior(typeof(ValidationPipelineBehavior<,>));
        });
        return services;
    }
}
