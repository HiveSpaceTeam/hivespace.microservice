namespace HiveSpace.Infrastructure.Messaging.Configurations;

public class AzureServiceBusOptions
{
    public const string SectionName = "Messaging:AzureServiceBus";

    public string ConnectionString { get; set; } = string.Empty;
}
