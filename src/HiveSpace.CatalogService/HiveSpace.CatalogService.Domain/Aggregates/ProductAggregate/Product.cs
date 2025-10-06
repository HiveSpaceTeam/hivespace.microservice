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
        public ProductStatus Status { get; private set; }

        private readonly List<ProductCategory> _categories = [];
        public IReadOnlyCollection<ProductCategory> Categories => _categories.AsReadOnly();

        private readonly List<ProductAttribute> _attributes = [];
        public IReadOnlyCollection<ProductAttribute> Attributes => _attributes.AsReadOnly();

        private readonly List<ProductImage> _images = [];
        public IReadOnlyCollection<ProductImage> Images => _images.AsReadOnly();

        public IReadOnlyCollection<Sku> Skus => _skus.AsReadOnly();
        private readonly List<Sku> _skus = [];

        public IReadOnlyCollection<ProductVariant> Variants => _variants.AsReadOnly();
        private readonly List<ProductVariant> _variants = [];

        public DateTimeOffset CreatedAt { get; private set; } = default!;
        public DateTimeOffset? UpdatedAt { get; private set; }
        public string CreatedBy { get; private set; } = default!;
        public string? UpdatedBy { get; private set; }

        #endregion


        #region Constructors

        // Parameterless constructor for Entity Framework
        private Product()
        {
            Name = string.Empty;
            Description = string.Empty;
            CreatedBy = string.Empty;
        }

        public Product(string name, string description, ProductStatus status, List<ProductCategory> categories, List<ProductAttribute> attributes, List<ProductImage> images, List<Sku> skus, List<ProductVariant> variants, DateTimeOffset createdAt, DateTimeOffset? updatedAt, string createdBy, string? updatedBy)
        {
            Name = name;
            Description = description;
            Status = status;
            _categories = categories;
            _attributes = attributes;
            _images = images;
            _skus = skus;
            _variants = variants;
            CreatedAt = createdAt;
            UpdatedAt = updatedAt;
            CreatedBy = createdBy;
            UpdatedBy = updatedBy;
        }

        public Product(Guid id, string name, string description, ProductStatus status, List<ProductCategory> categories, List<ProductAttribute> attributes, List<ProductImage> images, List<Sku> skus, List<ProductVariant> variants, DateTimeOffset createdAt, DateTimeOffset? updatedAt, string createdBy, string? updatedBy)
        {
            Id = id;
            Name = name;
            Description = description;
            Status = status;
            _categories = categories;
            _attributes = attributes;
            _images = images;
            _skus = skus;
            _variants = variants;
            CreatedAt = createdAt;
            UpdatedAt = updatedAt;
            CreatedBy = createdBy;
            UpdatedBy = updatedBy;
        }

        #endregion



        #region Methods
        private bool IsInvalid()
        {
            return _attributes is not null && _attributes.Count > 0 || _categories is not null && _categories.Count > 0;
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
