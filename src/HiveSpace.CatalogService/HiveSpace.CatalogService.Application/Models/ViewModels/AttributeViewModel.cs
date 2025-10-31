using HiveSpace.CatalogService.Domain.Aggregates.AttributeAggregate;

namespace HiveSpace.CatalogService.Application.Models.ViewModels;

public record AttributeViewModel(
    int Id,
    string Name,
    AttributeValueType ValueType,
    InputType InputType,
    bool IsMandatory,
    int MaxValueCount,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    List<AttributeValueViewModel> Values
);


