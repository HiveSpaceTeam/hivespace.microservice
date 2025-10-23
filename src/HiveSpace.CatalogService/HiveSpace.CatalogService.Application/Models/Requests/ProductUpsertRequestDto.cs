using HiveSpace.CatalogService.Domain.Common;

namespace HiveSpace.CatalogService.Application.Models.Requests;

public class ProductUpsertRequestDto
{
	public string Name { get; set; }
	public Guid Category { get; set; }
	public string Description { get; set; }
	public List<ProductVariantRequestDto>? Variants { get; set; }
	public List<ProductSkuRequestDto>? Skus { get; set; }
	public List<ProductAttributeRequestDto>? Attributes { get; set; }
}

public class ProductVariantRequestDto
{
	public Guid Id { get; set; }
	public string Name { get; set; } = string.Empty;
	public List<ProductVariantOptionRequestDto>? Options { get; set; }
}

public class ProductVariantOptionRequestDto
{
	public Guid OptionId { get; set; }
	public string Value { get; set; }
}

public class ProductSkuRequestDto
{
	public Guid Id { get; set; }
	public List<ProductSkuVariantRequestDto>? SkuVariants { get; set; }
	public Money Price { get; set; }
	public int Quantity { get; set; }
	public string SkuNo { get; set; }
}

public class ProductSkuVariantRequestDto
{
	public Guid VariantId { get; set; }
	public Guid OptionId { get; set; }
	public string Value { get; set; }
}

public class ProductAttributeRequestDto
{
	public Guid AttributeId { get; set; }
	public List<Guid>? SelectedValueIds { get; set; }
	public string? FreeTextValue { get; set; }
}



