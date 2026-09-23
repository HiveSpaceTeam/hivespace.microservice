using HiveSpace.CatalogService.Application.CatalogImports.Queueing;
using Microsoft.Azure.Functions.Worker;

namespace HiveSpace.CatalogService.Func.Functions.Queue;

public sealed class CatalogImportRabbitMqFunction(CatalogImportFunctionDispatcher dispatcher)
{
    [Function(nameof(CatalogImportRabbitMqFunction))]
    public Task RunAsync(
        [RabbitMQTrigger(
            CatalogImportQueueConstants.QueueName,
            ConnectionStringSetting = CatalogImportQueueConstants.RabbitMqConnectionSetting)]
        string message,
        CancellationToken cancellationToken)
        => dispatcher.HandleAsync(message, "RabbitMQ", cancellationToken);
}
