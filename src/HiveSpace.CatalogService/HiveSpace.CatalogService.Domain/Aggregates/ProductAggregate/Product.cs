using HiveSpace.Domain.Shared.Entities;
using HiveSpace.Domain.Shared.Interfaces;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.Domain.Shared.Errors;
using HiveSpace.CatalogService.Domain.Exceptions;
using HiveSpace.CatalogService.Domain.ValueObjects;
using HiveSpace.CatalogService.Domain.Enums;
using System.Text.Json.Serialization;

namespace HiveSpace.CatalogService.Domain.Aggregates.ProductAggregate
{
    public class Product : AggregateRoot<Guid>, IAuditable
    {
        #region Properties
        public Guid SellerId { get; private set; }
        public string Name { get; private set; }
        public string Slug { get; private set; }
        public string Description { get; private set; }
        public string? ShortDescription { get; private set; }
        
        public Guid? BrandId { get; private set; }
        
        public ProductStatus Status { get; private set; }
        public bool Featured { get; private set; }
        public ProductCondition Condition { get; private set; }

        public Weight? Weight { get; private set; }
        public Dimensions? Dimensions { get; private set; }

        private readonly List<ProductCategory> _categories = new();
        public IReadOnlyCollection<ProductCategory> Categories => _categories.AsReadOnly();

        private readonly List<ProductAttribute> _attributes = new();
        public IReadOnlyCollection<ProductAttribute> Attributes => _attributes.AsReadOnly();

        private readonly List<ProductImage> _images = new();
        public IReadOnlyCollection<ProductImage> Images => _images.AsReadOnly();

        public IReadOnlyCollection<Sku> Skus => _skus.AsReadOnly();
        private readonly List<Sku> _skus = new();

        public IReadOnlyCollection<ProductVariant> Variants => _variants.AsReadOnly();
        private readonly List<ProductVariant> _variants = new();

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
            Slug = string.Empty;
            Description = string.Empty;
            CreatedBy = string.Empty;
        }

        public Product(string name, string description, ProductStatus status, List<ProductCategory> categories, List<ProductAttribute> attributes, List<ProductImage> images, List<Sku> skus, List<ProductVariant> variants, DateTimeOffset createdAt, DateTimeOffset? updatedAt, string createdBy, string? updatedBy)
        {
            Name = name;
            Slug = string.Empty;
            Description = description;
            Status = status;
            if (categories is not null) _categories.AddRange(categories);
            if (attributes is not null) _attributes.AddRange(attributes);
            if (images is not null) _images.AddRange(images);
            if (skus is not null) _skus.AddRange(skus);
            if (variants is not null) _variants.AddRange(variants);
            CreatedAt = createdAt;
            UpdatedAt = updatedAt;
            CreatedBy = createdBy;
            UpdatedBy = updatedBy;
        }

        public Product(Guid id, string name, string description, ProductStatus status, List<ProductCategory> categories, List<ProductAttribute> attributes, List<ProductImage> images, List<Sku> skus, List<ProductVariant> variants, DateTimeOffset createdAt, DateTimeOffset? updatedAt, string createdBy, string? updatedBy)
        {
            Id = id;
            Name = name;
            Slug = string.Empty;
            Description = description;
            Status = status;
            if (categories is not null) _categories.AddRange(categories);
            if (attributes is not null) _attributes.AddRange(attributes);
            if (images is not null) _images.AddRange(images);
            if (skus is not null) _skus.AddRange(skus);
            if (variants is not null) _variants.AddRange(variants);
            CreatedAt = createdAt;
            UpdatedAt = updatedAt;
            CreatedBy = createdBy;
            UpdatedBy = updatedBy;
        }

        // Simplified constructor for creating new products without collections
        public Product(string name, string description, ProductStatus status, DateTimeOffset createdAt, string createdBy)
        {
            Id = Guid.NewGuid();
            Name = name;
            Slug = string.Empty;
            Description = description;
            Status = status;
            CreatedAt = createdAt;
            CreatedBy = createdBy;
            // Collections are already initialized as empty lists in field declarations
        }

        #endregion



        #region Methods
     
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
            if (category == null) throw new InvalidFieldException(DomainErrorCode.ParameterRequired, nameof(category));
            _categories.Add(category);
        }

        public void RemoveCategory(ProductCategory category)
        {
            if (category == null) throw new InvalidFieldException(DomainErrorCode.ParameterRequired, nameof(category));
            _categories.Remove(category);
        }

        public void UpdateCategories(List<ProductCategory> categories)
        {
            if (categories == null) throw new InvalidFieldException(DomainErrorCode.ParameterRequired, nameof(categories));
            _categories.Clear();
            _categories.AddRange(categories);
        }

        public void UpdateAttributes(List<ProductAttribute> attributes)
        {
            if (attributes == null) throw new InvalidFieldException(DomainErrorCode.ParameterRequired, nameof(attributes));
            _attributes.Clear();
            _attributes.AddRange(attributes);
        }

        public void UpdateVariants(List<ProductVariant> variants)
        {
            if (variants == null) throw new InvalidFieldException(DomainErrorCode.ParameterRequired, nameof(variants));
            _variants.Clear();
            _variants.AddRange(variants);
        }

        public void UpdateSkus(List<Sku> skus)
        {
            if (skus == null) throw new InvalidFieldException(DomainErrorCode.ParameterRequired, nameof(skus));
            _skus.Clear();
            _skus.AddRange(skus);
        }

        public void UpdateAuditInfo(string updatedBy)
        {
            UpdatedAt = DateTimeOffset.UtcNow;
            UpdatedBy = updatedBy;
        }
        #endregion
    }
}
