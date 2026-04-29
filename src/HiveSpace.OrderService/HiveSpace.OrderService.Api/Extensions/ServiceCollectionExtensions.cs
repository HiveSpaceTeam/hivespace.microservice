using HiveSpace.Core;
using HiveSpace.Core.OpenApi;
using HiveSpace.Domain.Shared.Converters;
using HiveSpace.Infrastructure.Authorization.Extensions;
using HiveSpace.Infrastructure.Messaging.Configurations;
using HiveSpace.Infrastructure.Messaging.Extensions;
using HiveSpace.OrderService.Api.Consumers.Saga.CheckoutSaga;
using HiveSpace.OrderService.Api.Consumers.Sync;
using HiveSpace.OrderService.Api.Sagas.CheckoutSaga;
using HiveSpace.OrderService.Api.Sagas.FulfillmentSaga;
using HiveSpace.OrderService.Infrastructure.Sagas;
using HiveSpace.OrderService.Infrastructure.Data;
using MassTransit;

namespace HiveSpace.OrderService.Api.Extensions;

internal static class ServiceCollectionExtensions
{
    public static void AddAppApiControllers(this IServiceCollection services)
    {
        services.AddHiveSpaceControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.Converters.Add(new EnumerationJsonConverterFactory());
                });

        services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.Converters.Add(new EnumerationJsonConverterFactory());
        });
    }

    public static void AddAppOpenApi(this IServiceCollection services)
        => services.AddHiveSpaceSwaggerGen("HiveSpace.OrderService API", "HiveSpace.OrderService microservice");

    public static void AddAppMessaging(this IServiceCollection services, IConfiguration configuration)
    {
        var messagingOptions = configuration.GetSection(MessagingOptions.SectionName).Get<MessagingOptions>();
        if (messagingOptions?.EnableRabbitMq != true) return;

        services.AddMassTransitWithRabbitMq<OrderDbContext>(configuration, cfg =>
        {
            cfg.AddSagaStateMachine<CheckoutSagaStateMachine, CheckoutSagaState>()
               .EntityFrameworkRepository(r =>
               {
                   r.ConcurrencyMode = ConcurrencyMode.Pessimistic;
                   r.ExistingDbContext<OrderDbContext>();
                   r.UseSqlServer();
               });
            cfg.AddSagaStateMachine<FulfillmentSagaStateMachine, FulfillmentSagaState>()
               .EntityFrameworkRepository(r =>
               {
                   r.ConcurrencyMode = ConcurrencyMode.Pessimistic;
                   r.ExistingDbContext<OrderDbContext>();
                   r.UseSqlServer();
               });
            cfg.AddConsumer<CreateOrderConsumer, CreateOrderConsumerDefinition>();
            cfg.AddConsumer<MarkOrderAsCODConsumer, MarkOrderAsCODConsumerDefinition>();
            cfg.AddConsumer<MarkOrderAsPaidConsumer, MarkOrderAsPaidConsumerDefinition>();
            cfg.AddConsumer<ClearCartConsumer, ClearCartConsumerDefinition>();
            cfg.AddConsumer<CancelOrderConsumer, CancelOrderConsumerDefinition>();
            cfg.AddConsumer<StoreRefSyncConsumer>();
            cfg.AddConsumer<ProductRefSyncConsumer>();
        });
    }

    public static void AddAppAuthentication(this IServiceCollection services, IConfiguration configuration)
        => services.AddHiveSpaceJwtBearerAuthentication(configuration, "order.fullaccess");

}
