using HiveSpace.CatalogService.Domain.CatalogImports;

namespace HiveSpace.CatalogService.Application.CatalogImports.Jobs;

public interface ICatalogImportJobScheduler
{
    Task ScheduleAsync(CatalogImportJob job, CancellationToken cancellationToken = default);
}
