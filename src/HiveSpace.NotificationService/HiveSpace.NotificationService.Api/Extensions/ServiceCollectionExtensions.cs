using HiveSpace.Core.OpenApi;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.Infrastructure.Authorization.Extensions;
using HiveSpace.Infrastructure.Messaging.Configurations;
using HiveSpace.Infrastructure.Messaging.Extensions;
using HiveSpace.NotificationService.Api.Consumers;
using HiveSpace.NotificationService.Api.Consumers.Sync;
using HiveSpace.NotificationService.Api.Hubs;
using Microsoft.AspNetCore.SignalR;
using HiveSpace.NotificationService.Core.BackgroundJobs;
using HiveSpace.NotificationService.Core.Infrastructure.Channels.Email;
using HiveSpace.NotificationService.Core.Infrastructure.Channels.InApp;
using HiveSpace.NotificationService.Core.Exceptions;
using HiveSpace.NotificationService.Core.Extensions;
using HiveSpace.NotificationService.Core.Interfaces;
using HiveSpace.NotificationService.Core.Persistence;
using HiveSpace.NotificationService.Core.Persistence.Repositories;
using HiveSpace.NotificationService.Core.Dispatch;
using HiveSpace.NotificationService.Core.Services;
using Hangfire;
using Hangfire.SqlServer;
using Microsoft.EntityFrameworkCore;
using Resend;
using Scalar.AspNetCore;
using StackExchange.Redis;

namespace HiveSpace.NotificationService.Api.Extensions;

internal static class ServiceCollectionExtensions
{
    public static void AddAppOpenApi(this IServiceCollection services)
        => services.AddHiveSpaceOpenApi("HiveSpace.NotificationService API", "HiveSpace.NotificationService microservice");

    public static void AddNotificationDbContext(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<NotificationDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
    }

    public static void AddAppSignalR(this IServiceCollection services)
    {
        services.AddSignalR();
        services.AddSingleton<IUserIdProvider, SubClaimUserIdProvider>();
        services.AddScoped<INotificationHubContext, NotificationHubContext>();
    }

    public static void AddAppRedis(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Redis")
            ?? throw new InvalidFieldException(NotificationDomainErrorCode.InvalidConfiguration, "Redis:ConnectionString");

        services.AddSingleton<IConnectionMultiplexer>(
            ConnectionMultiplexer.Connect(connectionString));

        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = connectionString;
        });
    }

    public static void AddAppHangfire(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHangfire(config =>
            config.UseSqlServerStorage(
                configuration.GetConnectionString("DefaultConnection"),
                new SqlServerStorageOptions
                {
                    CommandBatchMaxTimeout       = TimeSpan.FromMinutes(5),
                    SlidingInvisibilityTimeout   = TimeSpan.FromMinutes(5),
                    QueuePollInterval            = TimeSpan.Zero,
                    UseRecommendedIsolationLevel = true,
                    DisableGlobalLocks           = true,
                }));

        services.AddHangfireServer();
        services.AddSingleton<IBackgroundJobClient, BackgroundJobClient>();
    }

    public static void AddNotificationCoreServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMemoryCache();

        // Repositories
        services.AddScoped<INotificationRepository,         NotificationRepository>();
        services.AddScoped<IUserRefRepository,              UserRefRepository>();
        services.AddScoped<IUserPreferenceRepository,       UserPreferenceRepository>();
        services.AddScoped<INotificationTemplateRepository, NotificationTemplateRepository>();

        // Core services (Scoped)
        services.AddScoped<IDeduplicationService, DeduplicationService>();
        services.AddScoped<IRateLimiter,          RateLimiter>();
        services.AddScoped<ITemplateRenderer,     TemplateRenderer>();
        services.AddScoped<IChannelRouter,        ChannelRouter>();
        services.AddScoped<IDispatchPipeline,     NotificationDispatchPipeline>();

        // Retry scheduler (Singleton — wraps Hangfire IBackgroundJobClient which is Singleton)
        services.AddScoped<RetryNotificationJob>();
        services.AddSingleton<IRetryScheduler,    HangfireRetryScheduler>();

        // Channel providers
        services.AddScoped<IChannelProvider, InAppChannelProvider>();
        services.AddScoped<IChannelProvider, EmailChannelProvider>();

        // Resend email client
        services.AddOptions();
        services.AddHttpClient<ResendClient>();
        services.Configure<ResendClientOptions>(o =>
        {
            o.ApiToken = configuration["Resend:ApiToken"]
                ?? throw new InvalidFieldException(NotificationDomainErrorCode.InvalidConfiguration, "Resend:ApiToken");
        });
        services.AddTransient<IResend, ResendClient>();

        // Seed data
        services.AddNotificationSeedData();
    }

    public static void AddAppMessaging(this IServiceCollection services, IConfiguration configuration)
    {
        var messagingOptions = configuration.GetSection(MessagingOptions.SectionName).Get<MessagingOptions>();
        if (messagingOptions?.EnableRabbitMq != true) return;

        services.AddMassTransitWithRabbitMq<NotificationDbContext>(configuration, cfg =>
        {
            cfg.AddConsumer<NotifySellerNewOrderConsumer>();
            cfg.AddConsumer<NotifyBuyerOrderConfirmedConsumer>();
            cfg.AddConsumer<NotifyBuyerOrderCancelledConsumer>();
            cfg.AddConsumer<UserSyncConsumer>();
            cfg.AddConsumer<StoreSyncConsumer>();
        });
    }

    public static void AddAppAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHiveSpaceJwtBearerAuthentication(configuration, "notification.fullaccess", options =>
        {
            // SignalR WebSocket connections forward the token as ?access_token= since they cannot set HTTP headers.
            options.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    var token = context.Request.Query["access_token"].FirstOrDefault();
                    if (!string.IsNullOrEmpty(token) && context.Request.Path.StartsWithSegments("/hubs"))
                        context.Token = token;
                    return Task.CompletedTask;
                }
            };
            // With MapInboundClaims=false the "sub" claim is not remapped; treat it as the name claim for SignalR.
            options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
            {
                NameClaimType = "sub"
            };
        });
    }
}
