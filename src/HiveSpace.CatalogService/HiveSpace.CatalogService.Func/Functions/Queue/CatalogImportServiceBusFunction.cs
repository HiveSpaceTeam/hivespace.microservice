using HiveSpace.CatalogService.Application.CatalogImports.Queueing;
using Microsoft.Azure.Functions.Worker;

namespace HiveSpace.CatalogService.Func.Functions.Queue;

public sealed class CatalogImportServiceBusFunction(CatalogImportFunctionDispatcher dispatcher)
{
    [Function(nameof(CatalogImportServiceBusFunction))]
    public Task RunAsync(
        [ServiceBusTrigger(
            CatalogImportQueueConstants.QueueName,
            Connection = CatalogImportQueueConstants.AzureServiceBusConnectionSetting)]
        string message,
        CancellationToken cancellationToken)
        => dispatcher.HandleAsync(message, "AzureServiceBus", cancellationToken);
}
