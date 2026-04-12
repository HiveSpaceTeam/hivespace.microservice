using FluentValidation;
using HiveSpace.Core.Filters;
using HiveSpace.Domain.Shared.Converters;
using HiveSpace.Infrastructure.Authorization.Extensions;
using HiveSpace.Infrastructure.Messaging.Configurations;
using HiveSpace.Infrastructure.Messaging.Extensions;
using HiveSpace.OrderService.Api.Consumers.Saga.CheckoutSaga;
using HiveSpace.OrderService.Api.Consumers.Sync;
using HiveSpace.OrderService.Api.Sagas.CheckoutSaga;
using HiveSpace.OrderService.Api.Sagas.FulfillmentSaga;
using HiveSpace.OrderService.Application.Cart.Queries.GetCartItems;
using HiveSpace.OrderService.Infrastructure.Sagas;
using HiveSpace.OrderService.Application.Coupons.Commands.CreateCoupon;
using HiveSpace.OrderService.Infrastructure.Data;
using MassTransit;
using Microsoft.OpenApi.Models;

namespace HiveSpace.OrderService.Api.Extensions;

internal static class ServiceCollectionExtensions
{
    public static void AddAppApiControllers(this IServiceCollection services)
    {
        services.AddControllers(options =>
        {
            options.Filters.Add<CustomExceptionFilter>();
        });

        services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.Converters.Add(new EnumerationJsonConverterFactory());
        });
    }

    public static void AddAppSwagger(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title       = "HiveSpace.OrderService API",
                Version     = "v1",
                Description = "HiveSpace.OrderService microservice developed by Org"
            });

            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name        = "Authorization",
                Type        = SecuritySchemeType.Http,
                Scheme      = "bearer",
                BearerFormat = "JWT",
                In          = ParameterLocation.Header,
                Description = "Enter your JWT token in the format: Bearer {your token}"
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id   = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });
    }

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
            cfg.AddConsumer<NotifySellerConsumer>();
            cfg.AddConsumer<NotifyCustomerConsumer>();
            cfg.AddConsumer<StoreRefSyncConsumer>();
            cfg.AddConsumer<ProductRefSyncConsumer>();
        });
    }

    public static void AddAppAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthentication("Bearer")
            .AddJwtBearer("Bearer", options =>
            {
                options.Authority             = configuration["Authentication:Authority"];
                options.Audience              = configuration["Authentication:Audience"];
                options.RequireHttpsMetadata  = configuration.GetValue<bool>("Authentication:RequireHttpsMetadata", true);
                options.MapInboundClaims      = false;
            });
        services.AddHiveSpaceAuthorization("order.fullaccess");
    }

    public static void AddAppMediatR(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblyContaining<CreateCouponCommand>();
            cfg.RegisterServicesFromAssemblyContaining<OrderDbContext>();
        });
        services.AddScoped<IValidator<GetCartItemsQuery>, GetCartItemsQueryValidator>();
    }
}
