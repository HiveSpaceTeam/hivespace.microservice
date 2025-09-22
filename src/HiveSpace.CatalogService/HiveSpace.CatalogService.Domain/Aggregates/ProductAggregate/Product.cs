using HiveSpace.CatalogService.Domain.AggergateModels.ProductAggregate;
using HiveSpace.Domain.Shared.Entities;
using HiveSpace.Domain.Shared.Interfaces;
using System.Text.Json.Serialization;

namespace HiveSpace.CatalogService.Domain.Aggregates.ProductAggregate
{
    public class Product : AggregateRoot<Guid>, IAuditable
    {
        #region Properties
        public string Name { get; private set; }
        public string Description { get; private set; }
        //public ProductStatus Status { get; private set; }

        //private readonly List<ProductCategory> _categories = [];
        //public IReadOnlyCollection<ProductCategory> Categories => _categories.AsReadOnly();
        //private readonly List<ProductAttribute> _attributes = [];
        //public IReadOnlyCollection<ProductAttribute> Attributes => _attributes.AsReadOnly();
        //private readonly List<ProductImage> _images = [];
        //public IReadOnlyCollection<ProductImage> Images => _images.AsReadOnly();
        public IReadOnlyCollection<Sku> Skus { get; private set; }
        public IReadOnlyCollection<ProductVariant> Variants { get; private set; }
        public DateTimeOffset CreatedAt { get; private set; } = default!;
        public DateTimeOffset? UpdatedAt { get; private set; }
        public string CreatedBy { get; private set; } = default!;
        public string? UpdatedBy { get; private set; }
        #endregion

        #region Constructors
        [JsonConstructor]
        public Product(string name, string description, IReadOnlyCollection<Sku> skus, IReadOnlyCollection<ProductVariant> variants, DateTimeOffset createdAt, DateTimeOffset? updatedAt, string createdBy, string? updatedBy)
        {
            Name = name;
            Description = description;
            Skus = skus;
            Variants = variants;
            CreatedAt = createdAt;
            UpdatedAt = updatedAt;
            CreatedBy = createdBy;
            UpdatedBy = updatedBy;
        }

        private Product()
        {
        }

        // Old constructor commented out
        // public Product(string name, string description, List<ProductSku> skus, ProductStatus status, List<ProductCategory> categories, List<ProductAttribute> attributeValues)
        // {
        //     Name = name;
        //     Description = description;
        //     _skus = skus;

        //     //Status = status;
        //     //_categories = categories;
        //     //_attributes = attributeValues;

        //     //if (IsInvalid())
        //     //{
        //     //    throw new Exception("Invalid product");
        //     //}
        // }

        #endregion

        #region Methods
        //private bool IsInvalid()
        //{
        //    return _attributes is not null && _attributes.Count > 0 || _categories is not null && _categories.Count > 0;
        //}

        //public void UpdateName(string name)
        //{
        //    Name = name;
        //}

        //public void UpdateDescription(string description)
        //{
        //    Description = description;
        //}

        //public void AddCategory(ProductCategory category)
        //{
        //    if (category == null) throw new ArgumentNullException(nameof(category));
        //    _categories.Add(category);
        //}

        //public void RemoveCategory(ProductCategory category)
        //{
        //    if (category == null) throw new ArgumentNullException(nameof(category));
        //    _categories.Remove(category);
        //}
        #endregion
    }
}
