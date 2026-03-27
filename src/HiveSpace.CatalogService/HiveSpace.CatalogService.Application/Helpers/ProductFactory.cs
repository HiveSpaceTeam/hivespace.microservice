using HiveSpace.CatalogService.Application.Models.Requests;
using HiveSpace.CatalogService.Domain.Aggregates.ProductAggregate;

namespace HiveSpace.CatalogService.Application.Helpers;

/// <summary>
/// Factory class for creating Product-related domain entities from DTOs.
/// Centralizes the creation logic to avoid duplication across handlers and services.
/// </summary>
public static class ProductFactory
{
    /// <summary>
    /// Creates a list of ProductCategory entities from a category ID.
    /// </summary>
    public static List<ProductCategory> CreateProductCategories(int categoryId)
    {
        return categoryId > 0 ? [new ProductCategory(categoryId)] : [];
    }

    /// <summary>
    /// Creates a list of ProductVariant entities from variant request DTOs.
    /// </summary>
    public static List<ProductVariant> CreateProductVariants(ICollection<ProductVariantRequestDto>? variantRequests)
    {
        return variantRequests?.Select(CreateProductVariant).ToList() ?? [];
    }

    /// <summary>
    /// Creates a single ProductVariant entity from a variant request DTO.
    /// </summary>
    public static ProductVariant CreateProductVariant(ProductVariantRequestDto v)
    {
        var variant = new ProductVariant(v.Name);

        var options = v.Options?.Select(o => new ProductVariantOption(o.Value ?? string.Empty)).ToList() ?? [];

        variant.AddOptions(options);
        return variant;
    }

    /// <summary>
    /// Creates a list of Sku entities from SKU request DTOs.
    /// </summary>
    public static List<Sku> CreateProductSkus(ICollection<ProductSkuRequestDto>? skuRequests)
    {
        if (skuRequests is null) return [];

        return [.. skuRequests.Select(s =>
        {
            var skuId = s.Id != Guid.Empty ? s.Id : Guid.NewGuid();
            var skuVariants = s.SkuVariants?.Select(sv => new SkuVariant(sv.Value ?? string.Empty)).ToList() ?? [];
            return new Sku(s.SkuNo ?? string.Empty, skuVariants, [], s.Quantity, true, s.Price);
        })];
    }

    /// <summary>
    /// Creates a list of ProductAttribute entities from attribute request DTOs.
    /// </summary>
    public static List<ProductAttribute> CreateProductAttributes(ICollection<ProductAttributeRequestDto>? attributeRequests)
    {
        return attributeRequests?.Select(a => new ProductAttribute(a.AttributeId, a.SelectedValueIds, a.FreeTextValue)).ToList() ?? [];
    }
}


