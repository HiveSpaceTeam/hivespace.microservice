using HiveSpace.CatalogService.Application.Contracts;
using HiveSpace.CatalogService.Domain.Aggregates.ProductAggregate;

namespace HiveSpace.CatalogService.Application.Helpers;

public static class ProductFactory
{
    public static List<ProductCategory> CreateProductCategories(int categoryId)
        => categoryId > 0 ? [new ProductCategory(categoryId)] : [];

    public static List<ProductVariant> CreateProductVariants(ICollection<ProductVariantRequestDto>? variantRequests)
        => variantRequests?.Select(CreateProductVariant).ToList() ?? [];

    public static ProductVariant CreateProductVariant(ProductVariantRequestDto v)
    {
        var variant = new ProductVariant(v.Name);
        var options = v.Options?.Select(o => new ProductVariantOption(o.Value ?? string.Empty)).ToList() ?? [];
        variant.AddOptions(options);
        return variant;
    }

    public static List<Sku> CreateProductSkus(ICollection<ProductSkuRequestDto>? skuRequests)
    {
        if (skuRequests is null) return [];

        return [.. skuRequests.Select(s =>
        {
            var skuVariants = s.SkuVariants?.Select(sv => new SkuVariant(sv.VariantName, sv.Value ?? string.Empty)).ToList() ?? [];
            return new Sku(s.SkuNo ?? string.Empty, skuVariants, [], s.Quantity, true, s.Price);
        })];
    }

    public static List<ProductAttribute> CreateProductAttributes(ICollection<ProductAttributeRequestDto>? attributeRequests)
        => attributeRequests?.Select(a => new ProductAttribute(a.AttributeId, a.SelectedValueIds, a.FreeTextValue)).ToList() ?? [];
}
