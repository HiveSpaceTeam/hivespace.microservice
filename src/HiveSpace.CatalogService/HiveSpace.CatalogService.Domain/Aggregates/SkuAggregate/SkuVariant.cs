using HiveSpace.Domain.Shared.Entities;

namespace HiveSpace.CatalogService.Domain.Aggregates.SkuAggregate
{
    public class SkuVariant : ValueObject
    {
        public string Name { get; private set; }
        public string Value { get; private set; }

        public SkuVariant(string name, string value)
        {
            Name = name;
            Value = value;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Name;
            yield return Value;
        }
    }
}
