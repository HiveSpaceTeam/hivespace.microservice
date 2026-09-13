namespace HiveSpace.CatalogService.Domain.CatalogImports.Enums;

public enum ImportedImageMediaStatus
{
    ExternalOnly = 0,
    CopyRequested = 1,
    Copied = 2,
    Unsupported = 3,
    Inaccessible = 4,
    Duplicate = 5
}
