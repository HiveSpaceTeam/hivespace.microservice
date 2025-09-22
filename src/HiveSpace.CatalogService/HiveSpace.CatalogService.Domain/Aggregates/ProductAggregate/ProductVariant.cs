using HiveSpace.Domain.Shared.Entities;
using System.Text.Json.Serialization;

namespace HiveSpace.CatalogService.Domain.Aggregates.ProductAggregate
{
    public class ProductVariant : Entity<Guid>
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; }
        public IReadOnlyCollection<ProductVariantOption> Options { get; private set; }

        [JsonConstructor]
        public ProductVariant(Guid id, string name, IReadOnlyCollection<ProductVariantOption> options)
        {
            Id = id;
            Name = name;
            Options = options;
        }

        private ProductVariant() { }
    }
}
