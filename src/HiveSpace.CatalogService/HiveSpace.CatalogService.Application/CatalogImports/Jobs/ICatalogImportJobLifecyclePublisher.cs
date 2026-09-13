using HiveSpace.CatalogService.Domain.CatalogImports;

namespace HiveSpace.CatalogService.Application.CatalogImports.Jobs;

public interface ICatalogImportJobLifecyclePublisher
{
    Task PublishQueuedAsync(CatalogImportJob job, CancellationToken cancellationToken = default);
    Task PublishStartedAsync(CatalogImportJob job, CancellationToken cancellationToken = default);
    Task PublishProgressedAsync(CatalogImportJob job, CancellationToken cancellationToken = default);
    Task PublishCompletedAsync(CatalogImportJob job, CancellationToken cancellationToken = default);
    Task PublishFailedAsync(CatalogImportJob job, CancellationToken cancellationToken = default);
}
