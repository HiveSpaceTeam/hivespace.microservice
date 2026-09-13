namespace HiveSpace.CatalogService.Application.CatalogImports.Ports;

public record ImportedSellerAccountProvisioningResult(
    Guid? UserId,
    ImportedSellerProvisioningOutcome Outcome,
    string? ConflictReason);
