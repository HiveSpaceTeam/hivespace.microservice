namespace HiveSpace.CatalogService.Application.CatalogImports.Ports;

public interface IImportedSellerStoreClient
{
    Task<ImportedSellerStoreProvisioningResult> ProvisionAsync(
        ImportedSellerStoreProvisioningRequest request,
        CancellationToken cancellationToken = default);
}
