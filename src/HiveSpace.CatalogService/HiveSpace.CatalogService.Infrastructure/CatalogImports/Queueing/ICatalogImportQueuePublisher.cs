using HiveSpace.CatalogService.Application.CatalogImports.Queueing;

namespace HiveSpace.CatalogService.Infrastructure.CatalogImports.Queueing;

public interface ICatalogImportQueuePublisher
{
    Task PublishAsync(
        CatalogImportQueueWorkItem workItem,
        string payload,
        CancellationToken cancellationToken = default);
}
