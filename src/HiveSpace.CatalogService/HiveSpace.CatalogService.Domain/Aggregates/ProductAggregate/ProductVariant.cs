using HiveSpace.Domain.Shared.Entities;
using System.Text.Json.Serialization;

namespace HiveSpace.CatalogService.Domain.Aggregates.ProductAggregate
{
    public class ProductVariant : Entity<int>
    {
        public string Name { get; private set; }
        public IReadOnlyCollection<ProductVariantOption> Options => _options.AsReadOnly();
        private readonly List<ProductVariantOption> _options = [];

        // Parameterless constructor for Entity Framework
        private ProductVariant()
        {
            Name = string.Empty;
        }

        public ProductVariant(int id, string name, List<ProductVariantOption> options)
        {
            Id = id;
            Name = name;
            _options = options;
        }
        public ProductVariant(string name)
        {
            Name = name;
        }
        public void AddOption(string label)
        {
            _options.Add(new ProductVariantOption(label));
        }
        public void AddOptions(IEnumerable<ProductVariantOption> options)
        {
            _options.AddRange(options);
        }
    }
}
