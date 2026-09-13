namespace HiveSpace.CatalogService.Application.CatalogImports.Ports;

public record ImportedSellerStoreProvisioningResult(
    Guid? StoreId,
    ImportedSellerProvisioningOutcome Outcome,
    string? ConflictReason);
