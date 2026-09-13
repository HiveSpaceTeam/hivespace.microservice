namespace HiveSpace.CatalogService.Domain.CatalogImports;

public record ExternalCategoryAttributeValueLink(
    string SourceValueId,
    string Name,
    string DisplayName,
    int HiveSpaceAttributeValueId);
