using HiveSpace.Infrastructure.Messaging.Configurations;
using HiveSpace.Infrastructure.Messaging.Filters;
using HiveSpace.Infrastructure.Messaging.Observers;
using MassTransit;
using MassTransit.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Linq;
using MassTransit.EntityFrameworkCoreIntegration;
using Microsoft.EntityFrameworkCore;

namespace HiveSpace.Infrastructure.Messaging.Extensions;

public static class RabbitMqExtensions
{
    public static IServiceCollection AddMassTransitWithRabbitMq<TDbContext>(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<IBusRegistrationConfigurator>? configure = null)
        where TDbContext : DbContext
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

            bus.AddEntityFrameworkOutbox<TDbContext>(o =>
            {
                o.QueryDelay = TimeSpan.FromSeconds(1);
                o.UseSqlServer();
                o.UseBusOutbox();
                
            });

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

