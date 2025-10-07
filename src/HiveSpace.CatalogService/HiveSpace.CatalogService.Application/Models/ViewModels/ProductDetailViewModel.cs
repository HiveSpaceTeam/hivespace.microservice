using HiveSpace.CatalogService.Domain.Aggregates.ProductAggregate;

namespace HiveSpace.CatalogService.Application.Models.ViewModels
{
    public class ProductDetailViewModel
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public ProductStatus Status { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }
        public string CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
        public List<ProductCategoryViewModel> Categories { get; set; }
        public List<ProductAttribute> AttributeValues { get; set; }
        public List<ProductVariant> Variants { get; set; }
        public List<SkuImage> Images { get; set; }
        public List<Sku> Skus { get; set; }
    }

    public class ProductCategoryViewModel
    {
        public Guid CategoryId { get; set; }
        public string CategoryName { get; set; }
        public Guid? ParentId { get; set; }
    }
}
