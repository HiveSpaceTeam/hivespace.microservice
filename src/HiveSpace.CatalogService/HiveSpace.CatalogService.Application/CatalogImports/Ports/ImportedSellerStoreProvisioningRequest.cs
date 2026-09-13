namespace HiveSpace.CatalogService.Application.CatalogImports.Ports;

public record ImportedSellerStoreProvisioningRequest(
    string SourceSystem,
    string ExternalSellerId,
    Guid UserId,
    string StoreName,
    string? SourceUrl,
    string? LogoUrl = null);
