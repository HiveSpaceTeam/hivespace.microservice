using HiveSpace.Infrastructure.Messaging.Configurations;
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
        Action<IBusRegistrationConfigurator>? configure = null,
        Action<IBusRegistrationContext, IServiceBusBusFactoryConfigurator>? configureBus = null)
        where TDbContext : DbContext
    {
        services.AddMessagingCore(configuration);

        services.AddMassTransit(bus =>
        {
            configure?.Invoke(bus);

            bus.SetKebabCaseEndpointNameFormatter();
            bus.AddEntityFrameworkOutbox<TDbContext>(o =>
            {
                o.QueryDelay = TimeSpan.FromSeconds(1);
                o.UseSqlServer();
                o.UseBusOutbox();
            });

            bus.AddConfigureEndpointsCallback((context, name, cfg) => { cfg.UseEntityFrameworkOutbox<TDbContext>(context); });

            bus.UsingAzureServiceBus((context, cfg) =>
            {
                var options = context.GetRequiredService<IOptions<AzureServiceBusOptions>>().Value;

                cfg.Host(options.ConnectionString);

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
}
