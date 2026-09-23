namespace HiveSpace.CatalogService.Application.CatalogImports.Queueing;

public static class CatalogImportQueueConstants
{
    public const string QueueName = "catalog-import-jobs";
    public const string RabbitMqConnectionSetting = "ConnectionStrings:RabbitMq";
    public const string AzureServiceBusConnectionSetting = "ConnectionStrings:AzureServiceBus";
}
