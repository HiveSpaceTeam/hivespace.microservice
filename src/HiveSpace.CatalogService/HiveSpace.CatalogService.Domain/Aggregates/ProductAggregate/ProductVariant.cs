using HiveSpace.Domain.Shared.Entities;
using System.Text.Json.Serialization;

namespace HiveSpace.CatalogService.Domain.Aggregates.ProductAggregate
{
    public class ProductVariant : Entity<Guid>
    {
        public string Name { get; private set; }
        public IReadOnlyCollection<ProductVariantOption> Options => _options.AsReadOnly();
        private readonly List<ProductVariantOption> _options = [];

        // Parameterless constructor for Entity Framework
        private ProductVariant()
        {
            Name = string.Empty;
        }

        public ProductVariant(string name, List<ProductVariantOption> options)
        {
            Name = name;
            _options = options;
        }
    }
}
