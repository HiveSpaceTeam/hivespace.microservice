using HiveSpace.Infrastructure.Messaging.Configurations;
using MassTransit;
using MassTransit.KafkaIntegration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HiveSpace.Infrastructure.Messaging.Extensions;

public static class KafkaExtensions
{
    public static IServiceCollection AddMassTransitWithKafka(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<IRiderRegistrationConfigurator>? configureRider = null,
        Action<IKafkaFactoryConfigurator, IRiderRegistrationContext>? configureEndpoints = null)
    {
        services.AddMessagingCore(configuration);

        services.AddSingleton(new KafkaRegistration(configureRider, configureEndpoints));

        return services;
    }
}

