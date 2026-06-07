using Microsoft.Extensions.Configuration;

namespace HiveSpace.Infrastructure.Messaging.Configurations;

public static class MessagingConnectionStrings
{
    public const string RabbitMq = "RabbitMq";
    public const string Kafka = "Kafka";
    public const string AzureServiceBus = "AzureServiceBus";

    public static string GetRequired(IConfiguration configuration, string brokerName)
    {
        var connectionString = configuration.GetConnectionString(brokerName);

        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException(
                $"Messaging broker '{brokerName}' is enabled but ConnectionStrings:{brokerName} is missing or empty.");

        return connectionString;
    }
}
