using HiveSpace.Infrastructure.Messaging.Configurations;
using HiveSpace.Infrastructure.Messaging.Diagnostics;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace HiveSpace.Infrastructure.Messaging.Extensions;

public static class AzureServiceBusExtensions
{
    public static IServiceCollection AddMassTransitWithAzureServiceBus<TDbContext>(
        this IServiceCollection services,
        IConfiguration configuration,
        string servicePrefix,
        Action<IBusRegistrationConfigurator>? configure = null,
        Action<IBusRegistrationContext, IServiceBusBusFactoryConfigurator>? configureBus = null)
        where TDbContext : DbContext
    {
        services.AddMessagingCore(configuration);
        ValidateServicePrefix(servicePrefix);

        services.AddMassTransit(bus =>
        {
            configure?.Invoke(bus);

            bus.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter(servicePrefix, false));
            bus.AddEntityFrameworkOutbox<TDbContext>(o =>
            {
                o.QueryDelay = TimeSpan.FromSeconds(1);
                o.UseSqlServer();
                o.UseBusOutbox();
            });

            bus.AddConfigureEndpointsCallback((context, name, cfg) => { cfg.UseEntityFrameworkOutbox<TDbContext>(context); });

            bus.UsingAzureServiceBus((context, cfg) =>
            {
                var connectionString = MessagingConnectionStrings.GetRequired(
                    configuration,
                    MessagingConnectionStrings.AzureServiceBus);

                cfg.Host(connectionString);
                cfg.UseSendFilter(typeof(MassTransitTraceSendFilter<>), context);
                cfg.UsePublishFilter(typeof(MassTransitTracePublishFilter<>), context);
                cfg.UseConsumeFilter(typeof(MassTransitTraceConsumeFilter<>), context);

                if (configureBus != null)
                {
                    configureBus.Invoke(context, cfg);
                }
                else
                {
                    cfg.ConfigureEndpoints(context);
                }
            });
        });

        return services;
    }

    private static void ValidateServicePrefix(string servicePrefix)
    {
        if (string.IsNullOrWhiteSpace(servicePrefix))
            throw new ArgumentException("Service prefix is required.", nameof(servicePrefix));
    }
}
