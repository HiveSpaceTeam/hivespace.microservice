using HiveSpace.Infrastructure.Messaging.Configurations;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

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

        //var kafkaRegistrations = services
        //    .Where(sd => sd.ServiceType == typeof(KafkaRegistration) && sd.ImplementationInstance is KafkaRegistration)
        //    .Select(sd => (KafkaRegistration)sd.ImplementationInstance!)
        //    .ToList();

        services.AddMassTransit(bus =>
        {
            configure?.Invoke(bus);

            bus.AddDelayedMessageScheduler();

            //foreach (var registration in kafkaRegistrations)
            //{
            //    bus.AddRider(rider =>
            //    {
            //        registration.ConfigureRider?.Invoke(rider);
            //        rider.UsingKafka((context, kafka) =>
            //        {
            //            var kafkaOptions = context.GetRequiredService<IOptions<KafkaOptions>>().Value;
            //            kafka.Host(kafkaOptions.BootstrapServers);

            //            registration.ConfigureEndpoints?.Invoke(kafka, context);
            //        });
            //    });
            //}

            bus.SetKebabCaseEndpointNameFormatter();
            bus.AddEntityFrameworkOutbox<TDbContext>(o =>
            {
                var rabbitMqOptions = configuration.GetSection(RabbitMqOptions.SectionName).Get<RabbitMqOptions>() ?? new RabbitMqOptions();

                // Keep inbox rows only long enough for duplicate detection so cleanup
                // doesn't have to churn through a large backlog under load.
                o.DuplicateDetectionWindow = TimeSpan.FromMinutes(GetPositiveOrDefault(rabbitMqOptions.DuplicateDetectionWindowMinutes, 5));
                o.QueryDelay = TimeSpan.FromSeconds(GetPositiveOrDefault(rabbitMqOptions.OutboxQueryDelaySeconds, 1));
                o.QueryMessageLimit = GetPositiveOrDefault(rabbitMqOptions.OutboxQueryMessageLimit, 100);
                o.QueryTimeout = TimeSpan.FromSeconds(GetPositiveOrDefault(rabbitMqOptions.OutboxQueryTimeoutSeconds, 60));
                o.UseSqlServer();
                o.UseBusOutbox(busOutbox =>
                {
                    busOutbox.MessageDeliveryLimit = GetPositiveOrDefault(rabbitMqOptions.OutboxMessageDeliveryLimit, 50);
                });
            });

            bus.AddConfigureEndpointsCallback((context, name, cfg) =>
            {
                cfg.UseMessageRetry(r => r.Intervals(500, 1000, 2000, 5000));
                cfg.UseEntityFrameworkOutbox<TDbContext>(context);
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

                cfg.UseDelayedMessageScheduler();
                cfg.PrefetchCount = options.PrefetchCount;
                cfg.ConfigureEndpoints(context);
            });
        });

        return services;
    }

    private static int GetPositiveOrDefault(int value, int defaultValue)
        => value > 0 ? value : defaultValue;
}
