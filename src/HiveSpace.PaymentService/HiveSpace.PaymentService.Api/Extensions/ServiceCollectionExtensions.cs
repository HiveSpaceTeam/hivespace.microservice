using HiveSpace.Core;
using HiveSpace.Core.OpenApi;
using HiveSpace.Infrastructure.Authorization.Extensions;
using HiveSpace.Infrastructure.Messaging.Configurations;
using HiveSpace.Infrastructure.Messaging.Extensions;
using HiveSpace.PaymentService.Api.Consumers.Saga.CheckoutSaga;
using HiveSpace.PaymentService.Infrastructure;
using HiveSpace.PaymentService.Infrastructure.Data;
using MassTransit;

namespace HiveSpace.PaymentService.Api.Extensions;

internal static class ServiceCollectionExtensions
{
    public static void AddAppApiControllers(this IServiceCollection services)
        => services.AddHiveSpaceControllers();

    public static void AddAppOpenApi(this IServiceCollection services)
        => services.AddHiveSpaceSwaggerGen("HiveSpace.PaymentService API", "HiveSpace.PaymentService microservice");

    public static void AddAppMessaging(this IServiceCollection services, IConfiguration configuration)
    {
        var messagingOptions = configuration.GetSection(MessagingOptions.SectionName).Get<MessagingOptions>();
        if (messagingOptions?.EnableRabbitMq != true) return;

        services.AddMassTransitWithRabbitMq<PaymentDbContext>(configuration, cfg =>
        {
            cfg.AddConsumer<InitiatePaymentConsumer>();
        });
    }

    public static void AddAppAuthentication(this IServiceCollection services, IConfiguration configuration)
        => services.AddHiveSpaceJwtBearerAuthentication(configuration, "payment.fullaccess");
}
