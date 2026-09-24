namespace HiveSpace.CatalogService.Application.CatalogImports.Queueing;

public interface ICatalogImportJobExecutionGate
{
    Task<CatalogImportJobClaimResult> TryClaimAsync(
        CatalogImportQueueWorkItem workItem,
        CancellationToken cancellationToken = default);
}
