using HiveSpace.CatalogService.Domain.Aggregates.ProductAggregate;

namespace HiveSpace.CatalogService.Application.Models.ViewModels
{
    public class ProductDetailViewModel
    {
        public int Id { get; set; }
        public Guid SellerId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public List<ProductCategory> Categories { get; set; }

        public List<ProductImage> Images { get; set; }

        public List<ProductAttributeViewModel> Attributes { get; set; }

        public List<Sku> Skus { get; set; }

        public List<ProductVariant> Variants { get; set; }

        public CurrentSeller CurrentSeller { get; set; }
    }

    public class CurrentSeller
    {
        public Guid Id { get; set; }
        public string StoreName { get; set; }
        public string LogoUrl { get; set; }
    }
}
