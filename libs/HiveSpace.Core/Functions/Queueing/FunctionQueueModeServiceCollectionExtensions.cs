using HiveSpace.Core.Exceptions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HiveSpace.Core.Functions.Queueing;

public static class FunctionQueueModeServiceCollectionExtensions
{
    public static IServiceCollection AddFunctionQueueMode(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var options = FunctionQueueModeOptions.FromConfiguration(configuration);
        var validationResult = options.Validate();

        if (!validationResult.Succeeded)
            throw new ConfigurationException(validationResult.Errors);

        services.AddSingleton(options);
        services.AddSingleton(typeof(FunctionQueueMode), validationResult.Mode!.Value);

        return services;
    }

    public static bool IsRabbitMqMode(this FunctionQueueMode mode)
        => mode == FunctionQueueMode.RabbitMQ;

    public static bool IsAzureServiceBusMode(this FunctionQueueMode mode)
        => mode == FunctionQueueMode.AzureServiceBus;
}
