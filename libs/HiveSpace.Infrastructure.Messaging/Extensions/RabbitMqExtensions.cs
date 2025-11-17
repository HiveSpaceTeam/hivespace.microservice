using System.Linq;
using HiveSpace.Infrastructure.Messaging.Filters;
using HiveSpace.Infrastructure.Messaging.Observers;
using HiveSpace.Infrastructure.Messaging.Configurations;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace HiveSpace.Infrastructure.Messaging.Extensions;

public static class RabbitMqExtensions
{
    public static IServiceCollection AddMassTransitWithRabbitMq(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<IBusRegistrationConfigurator>? configure = null)
    {
        services.AddMessagingCore(configuration);

        var kafkaRegistrations = services
            .Where(sd => sd.ServiceType == typeof(KafkaRegistration) && sd.ImplementationInstance is KafkaRegistration)
            .Select(sd => (KafkaRegistration)sd.ImplementationInstance!)
            .ToList();

        services.AddMassTransit(bus =>
        {
            configure?.Invoke(bus);

            foreach (var registration in kafkaRegistrations)
            {
                bus.AddRider(rider =>
                {
                    registration.ConfigureRider?.Invoke(rider);
                    rider.UsingKafka((context, kafka) =>
                    {
                        var kafkaOptions = context.GetRequiredService<IOptions<KafkaOptions>>().Value;
                        kafka.Host(kafkaOptions.BootstrapServers);

                        registration.ConfigureEndpoints?.Invoke(kafka, context);
                    });
                });
            }

            bus.SetKebabCaseEndpointNameFormatter();

            bus.UsingRabbitMq((context, cfg) =>
            {
                var options = context.GetRequiredService<IOptions<RabbitMqOptions>>().Value;

                cfg.Host(options.Host, options.Port, options.VirtualHost, host =>
                {
                    host.Username(options.Username);
                    host.Password(options.Password);
                    host.Heartbeat(options.HeartBeat);
                    if (options.UseSsl)
                    {
                        host.UseSsl(ssl =>
                        {
                            ssl.Protocol = System.Security.Authentication.SslProtocols.Tls12;
                        });
                    }
                });

                cfg.PrefetchCount = options.PrefetchCount;

                cfg.UseConsumeFilter(typeof(LoggingFilter<>), context);
                cfg.UseConsumeFilter(typeof(ExceptionHandlingFilter<>), context);
                cfg.UseConsumeFilter(typeof(RetryFilter<>), context);

                cfg.ConnectPublishObserver(context.GetRequiredService<PublishObserver>());
                cfg.ConnectSendObserver(context.GetRequiredService<SendObserver>());
                cfg.ConnectConsumeObserver(context.GetRequiredService<ConsumeObserver>());

                cfg.ConfigureEndpoints(context);
            });
        });

        return services;
    }
}

