namespace HiveSpace.CatalogService.Domain.CatalogImports.Enums;

public enum CatalogImportBundleStatus
{
    Submitted = 0,
    Validated = 1,
    NeedsAttention = 2,
    ProvisioningSellers = 3,
    ReadyToImport = 4,
    PartiallyImported = 5,
    Imported = 6,
    Failed = 7
}
