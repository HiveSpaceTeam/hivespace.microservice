using HiveSpace.CatalogService.Domain.Common;

namespace HiveSpace.CatalogService.Application.Models.Requests;

public class ProductUpsertRequest
{
	public string? Name { get; set; }
	public Guid Category { get; set; }
	public string? Description { get; set; }
	public List<ProductVariantRequest>? Variants { get; set; }
	public List<ProductSkuRequest>? Skus { get; set; }
	public List<ProductAttributeRequest>? Attributes { get; set; }
}

public class ProductVariantRequest
{
	public Guid Id { get; set; }
	public string Name { get; set; } = string.Empty;
	public List<ProductVariantOptionRequest>? Options { get; set; }
}

public class ProductVariantOptionRequest
{
	public Guid OptionId { get; set; }
	public string? Value { get; set; }
}

public class ProductSkuRequest
{
	public Guid Id { get; set; }
	public List<ProductSkuVariantRequest>? SkuVariants { get; set; }
	public Money Price { get; set; }
	public int Quantity { get; set; }
	public string? SkuNo { get; set; }
}

public class ProductSkuVariantRequest
{
	public Guid VariantId { get; set; }
	public Guid OptionId { get; set; }
	public string? Value { get; set; }
}

public class ProductAttributeRequest
{
	public Guid AttributeId { get; set; }
	public List<Guid>? SelectedValueIds { get; set; }
	public string? FreeTextValue { get; set; }
}



