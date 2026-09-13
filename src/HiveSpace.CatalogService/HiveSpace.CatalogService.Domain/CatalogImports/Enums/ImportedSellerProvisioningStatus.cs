namespace HiveSpace.CatalogService.Domain.CatalogImports.Enums;

public enum ImportedSellerProvisioningStatus
{
    Unmatched = 0,
    Matched = 1,
    CreateRequested = 2,
    Created = 3,
    Conflict = 4,
    Failed = 5
}
