using HiveSpace.CatalogService.Domain.Common;

namespace HiveSpace.CatalogService.Application.Models.Requests;

public record ProductUpsertRequestDto(
	string Name,
	int Category,
	string Description,
	List<ProductVariantRequestDto>? Variants = null,
	List<ProductSkuRequestDto>? Skus = null,
	List<ProductAttributeRequestDto>? Attributes = null
);

public record ProductVariantRequestDto(
	Guid Id,
	string Name,
	List<ProductVariantOptionRequestDto>? Options = null
);

public record ProductVariantOptionRequestDto(
	Guid OptionId,
	string Value
);

public record ProductSkuRequestDto(
	Guid Id,
	List<ProductSkuVariantRequestDto>? SkuVariants,
	Money Price,
	int Quantity,
	string SkuNo
);

public record ProductSkuVariantRequestDto(
	Guid VariantId,
	Guid OptionId,
	string Value
);

public record ProductAttributeRequestDto(
	int AttributeId,
	List<int>? SelectedValueIds = null,
	string? FreeTextValue = null
);


