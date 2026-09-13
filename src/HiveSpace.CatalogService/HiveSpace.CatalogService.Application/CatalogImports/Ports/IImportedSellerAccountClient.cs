namespace HiveSpace.CatalogService.Application.CatalogImports.Ports;

public interface IImportedSellerAccountClient
{
    Task<ImportedSellerAccountProvisioningResult> ProvisionAsync(
        ImportedSellerAccountProvisioningRequest request,
        CancellationToken cancellationToken = default);
}
