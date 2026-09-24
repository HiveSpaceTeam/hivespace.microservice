using HiveSpace.CatalogService.Application.CatalogImports.Jobs;
using HiveSpace.CatalogService.Domain.CatalogImports;

namespace HiveSpace.CatalogService.Tests.Application.CatalogImports;

internal sealed class NullCatalogImportJobScheduler : ICatalogImportJobScheduler
{
    public Task ScheduleAsync(CatalogImportJob job, CancellationToken cancellationToken = default)
        => Task.CompletedTask;
}
