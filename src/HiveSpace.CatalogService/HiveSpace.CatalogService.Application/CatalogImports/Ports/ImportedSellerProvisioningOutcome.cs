namespace HiveSpace.CatalogService.Application.CatalogImports.Ports;

public enum ImportedSellerProvisioningOutcome
{
    Created = 0,
    Matched = 1,
    Conflict = 2,
    Failed = 3
}
