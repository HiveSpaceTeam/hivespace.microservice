using HiveSpace.CatalogService.Application.CatalogImports.Jobs;
using HiveSpace.CatalogService.Domain.CatalogImports;

namespace HiveSpace.CatalogService.Tests.Application.CatalogImports;

internal sealed class NullCatalogImportJobLifecyclePublisher : ICatalogImportJobLifecyclePublisher
{
    public Task PublishQueuedAsync(CatalogImportJob job, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    public Task PublishStartedAsync(CatalogImportJob job, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    public Task PublishProgressedAsync(CatalogImportJob job, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    public Task PublishCompletedAsync(CatalogImportJob job, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    public Task PublishFailedAsync(CatalogImportJob job, CancellationToken cancellationToken = default)
        => Task.CompletedTask;
}
