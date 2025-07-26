using HiveSpace.CatalogService.Domain.Aggregates.SkuAggregate;
using HiveSpace.Domain.Shared.Entities;
using HiveSpace.Domain.Shared.Interfaces;

namespace HiveSpace.CatalogService.Domain.Aggregates.ProductAggregate
{
    public class Product : AggregateRoot<int>, IAuditable
    {
        #region Properties
        public string Name { get; private set; }
        public string Description { get; private set; }
        public ProductStatus Status { get; private set; }

        private readonly List<ProductCategory> _categories = [];
        public IReadOnlyCollection<ProductCategory> Categories => _categories.AsReadOnly();

        //private readonly List<ProductAttributeValue> _attributeValues = [];
        //public IReadOnlyCollection<ProductAttributeValue> AttributeValues => _attributeValues.AsReadOnly();

        private readonly List<ProductVariant> _variants = [];
        public IReadOnlyCollection<ProductVariant> Variants => _variants.AsReadOnly();

        private readonly List<ProductImage> _images = [];
        public IReadOnlyCollection<ProductImage> Images => _images.AsReadOnly();

        private readonly List<Sku> _skus = [];
        public IReadOnlyCollection<Sku> Skus => _skus.AsReadOnly();

        public DateTimeOffset CreatedAt { get; private set; } = default!;
        public DateTimeOffset? UpdatedAt { get; private set; }
        public string CreatedBy { get; private set; } = default!;
        public string? UpdatedBy { get; private set; }
        #endregion

        #region Constructors
        private Product()
        {
        }

        public Product(string name, string description, ProductStatus status, List<ProductCategory> categories, List<ProductAttributeValue> attributeValues, List<ProductVariant> productVariants, List<ProductImage> images)
        {
            Name = name;
            Description = description;
            Status = status;
            _categories = categories;
            _attributeValues = attributeValues;
            _variants = productVariants;
            _images = images;

            if (IsInvalid())
            {
                throw new Exception("Invalid product");
            }
        }

        #endregion

        #region Methods
        private bool IsInvalid()
        {
            return _attributeValues is not null && _attributeValues.Count > 0 || _categories is not null && _categories.Count > 0;
        }

        public void UpdateName(string name)
        {
            Name = name;
        }

        public void UpdateDescription(string description)
        {
            Description = description;
        }

        public void AddCategory(ProductCategory category)
        {
            if (category == null) throw new ArgumentNullException(nameof(category));
            _categories.Add(category);
        }

        public void RemoveCategory(ProductCategory category)
        {
            if (category == null) throw new ArgumentNullException(nameof(category));
            _categories.Remove(category);
        }
        #endregion
    }
}
