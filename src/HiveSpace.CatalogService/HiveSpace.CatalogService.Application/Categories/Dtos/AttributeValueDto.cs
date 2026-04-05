namespace HiveSpace.CatalogService.Application.Categories.Dtos;

public record AttributeValueDto(
    int Id,
    int AttributeId,
    string Name,
    string DisplayName,
    int? ParentValueId,
    bool IsActive,
    int SortOrder
);
