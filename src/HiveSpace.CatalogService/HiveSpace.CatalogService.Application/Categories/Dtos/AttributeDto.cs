using HiveSpace.CatalogService.Domain.Aggregates.AttributeAggregate;

namespace HiveSpace.CatalogService.Application.Categories.Dtos;

public record AttributeDto(
    int Id,
    string Name,
    AttributeValueType ValueType,
    InputType InputType,
    bool IsMandatory,
    int MaxValueCount,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    List<AttributeValueDto> Values
);
