using HiveSpace.Infrastructure.Messaging.Configurations;
using HiveSpace.Infrastructure.Messaging.Diagnostics;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Quartz;

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

        // Configure Quartz
        services.AddQuartz();

        //var kafkaRegistrations = services
        //    .Where(sd => sd.ServiceType == typeof(KafkaRegistration) && sd.ImplementationInstance is KafkaRegistration)
        //    .Select(sd => (KafkaRegistration)sd.ImplementationInstance!)
        //    .ToList();

        services.AddMassTransit(bus =>
        {
            configure?.Invoke(bus);

            bus.AddPublishMessageScheduler();
            bus.AddQuartzConsumers();

            //foreach (var registration in kafkaRegistrations)
            //{
            //    bus.AddRider(rider =>
            //    {
            //        registration.ConfigureRider?.Invoke(rider);
            //        rider.UsingKafka((context, kafka) =>
            //        {
            //            var kafkaOptions = context.GetRequiredService<IOptions<KafkaOptions>>().Value;
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
                var connectionString = MessagingConnectionStrings.GetRequired(configuration, MessagingConnectionStrings.RabbitMq);
                var connectionUri = new Uri(connectionString);
                var credentials = ParseCredentials(connectionUri);
                var virtualHost = string.IsNullOrWhiteSpace(connectionUri.AbsolutePath.Trim('/'))
                    ? "/"
                    : Uri.UnescapeDataString(connectionUri.AbsolutePath.Trim('/'));
                var port = connectionUri.Port > 0 ? (ushort)connectionUri.Port : (ushort)5672;

                cfg.Host(connectionUri.Host, port, virtualHost, host =>
                {
                    if (credentials is not null)
                    {
                        host.Username(credentials.Value.UserName);
                        host.Password(credentials.Value.Password);
                    }

                    host.Heartbeat(options.HeartBeat);
                    if (string.Equals(connectionUri.Scheme, "amqps", StringComparison.OrdinalIgnoreCase))
                    {
                        host.UseSsl(ssl =>
                        {
                            ssl.Protocol = System.Security.Authentication.SslProtocols.Tls12;
                        });
                    }
                });

                cfg.UsePublishMessageScheduler();
                cfg.UseSendFilter(typeof(MassTransitTraceSendFilter<>), context);
                cfg.UsePublishFilter(typeof(MassTransitTracePublishFilter<>), context);
                cfg.UseConsumeFilter(typeof(MassTransitTraceConsumeFilter<>), context);
                cfg.PrefetchCount = options.PrefetchCount;
                cfg.ConfigureEndpoints(context);
            });
        });

        return services;
    }

    private static int GetPositiveOrDefault(int value, int defaultValue)
        => value > 0 ? value : defaultValue;

    private static (string UserName, string Password)? ParseCredentials(Uri uri)
    {
        if (string.IsNullOrWhiteSpace(uri.UserInfo))
            return null;

        var parts = uri.UserInfo.Split(':', 2);
        var userName = Uri.UnescapeDataString(parts[0]);
        var password = parts.Length > 1 ? Uri.UnescapeDataString(parts[1]) : string.Empty;

        return (userName, password);
    }
}
