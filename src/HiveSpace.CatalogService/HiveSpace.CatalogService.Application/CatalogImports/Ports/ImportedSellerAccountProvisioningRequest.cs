namespace HiveSpace.CatalogService.Application.CatalogImports.Ports;

public record ImportedSellerAccountProvisioningRequest(
    string SourceSystem,
    string ExternalSellerId,
    string DisplayName,
    string? SourceUrl);
