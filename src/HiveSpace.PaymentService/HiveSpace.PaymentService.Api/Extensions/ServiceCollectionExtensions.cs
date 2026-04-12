using HiveSpace.Core.Filters;
using HiveSpace.Infrastructure.Authorization.Extensions;
using HiveSpace.Infrastructure.Messaging.Configurations;
using HiveSpace.Infrastructure.Messaging.Extensions;
using HiveSpace.PaymentService.Api.Consumers.Saga.CheckoutSaga;
using HiveSpace.PaymentService.Application.Payments.Commands.ProcessPaymentWebhook;
using HiveSpace.PaymentService.Infrastructure;
using HiveSpace.PaymentService.Infrastructure.Data;
using MassTransit;
using Microsoft.OpenApi.Models;

namespace HiveSpace.PaymentService.Api.Extensions;

internal static class ServiceCollectionExtensions
{
    public static void AddAppApiControllers(this IServiceCollection services)
    {
        services.AddControllers(options =>
        {
            options.Filters.Add<CustomExceptionFilter>();
        });
    }

    public static void AddAppSwagger(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title       = "HiveSpace.PaymentService API",
                Version     = "v1",
                Description = "HiveSpace.PaymentService microservice"
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

        services.AddMassTransitWithRabbitMq<PaymentDbContext>(configuration, cfg =>
        {
            cfg.AddConsumer<InitiatePaymentConsumer>();
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
        services.AddHiveSpaceAuthorization("payment.fullaccess");
    }

    public static void AddAppMediatR(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblyContaining<ProcessPaymentWebhookCommand>();
        });
    }
}
