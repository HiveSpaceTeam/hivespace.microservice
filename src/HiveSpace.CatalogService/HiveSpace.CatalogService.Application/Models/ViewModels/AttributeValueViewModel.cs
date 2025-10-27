namespace HiveSpace.CatalogService.Application.Models.ViewModels;

public record AttributeValueViewModel(
    int Id,
    int AttributeId,
    string Name,
    string DisplayName,
    int? ParentValueId,
    bool IsActive,
    int SortOrder
);


