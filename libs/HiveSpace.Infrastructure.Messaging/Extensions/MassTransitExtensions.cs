using HiveSpace.Infrastructure.Messaging.Abstractions;
using HiveSpace.Infrastructure.Messaging.Configurations;
using HiveSpace.Infrastructure.Messaging.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace HiveSpace.Infrastructure.Messaging.Extensions;

/// <summary>
/// Provides shared registration helpers for HiveSpace messaging.
/// </summary>
public static class MassTransitExtensions
{
    public static IServiceCollection AddMessagingCore(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<MessagingOptions>(configuration.GetSection(MessagingOptions.SectionName));
        services.Configure<RabbitMqOptions>(configuration.GetSection(RabbitMqOptions.SectionName));
        services.Configure<KafkaOptions>(configuration.GetSection(KafkaOptions.SectionName));

        services.TryAddScoped<IMessageBus, MassTransitMessageBus>();
        services.TryAddScoped<IEventPublisher>(sp => (IEventPublisher)sp.GetRequiredService<IMessageBus>());

        return services;
    }
}

