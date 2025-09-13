using HiveSpace.Domain.Shared.Entities;

namespace HiveSpace.CatalogService.Domain.Aggregates.SkuAggregate
{
    public class SkuVariant : ValueObject
    {
        public int SkuId { get; set; }
        public int VariantId { get; set; }
        public string Value { get; private set; }

        public SkuVariant(int skuId, int variantId, string value)
        {
            SkuId = skuId;
            VariantId = variantId;
            Value = value;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return SkuId;
            yield return VariantId;
            yield return Value;
        }
    }
}
