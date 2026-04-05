using HiveSpace.Domain.Shared.ValueObjects;

namespace HiveSpace.CatalogService.Application.Contracts;

public record ProductUpsertRequestDto(
    string Name,
    int Category,
    string Description,
    List<ProductVariantRequestDto>? Variants = null,
    List<ProductSkuRequestDto>? Skus = null,
    List<ProductAttributeRequestDto>? Attributes = null
);

public record ProductVariantRequestDto(
    int Id,
    string Name,
    List<ProductVariantOptionRequestDto>? Options = null
);

public record ProductVariantOptionRequestDto(string Value);

public record ProductSkuRequestDto(
    int Id,
    List<ProductSkuVariantRequestDto>? SkuVariants,
    Money Price,
    int Quantity,
    string SkuNo
);

public record ProductSkuVariantRequestDto(string VariantName, string Value);

public record ProductAttributeRequestDto(
    int AttributeId,
    List<int>? SelectedValueIds = null,
    string? FreeTextValue = null
);
