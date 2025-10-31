using HiveSpace.Domain.Shared.Entities;
using System.Text.Json.Serialization;

namespace HiveSpace.CatalogService.Domain.Aggregates.ProductAggregate
{
    public class SkuVariant : ValueObject
    {
        public Guid SkuId { get; private set; }
        public Guid VariantId { get; private set; }
        public Guid OptionId { get; private set; }
        public string Value { get; private set; }

        public SkuVariant(Guid skuId, Guid variantId, Guid optionId, string value)
        {
            SkuId = skuId;
            VariantId = variantId;
            OptionId = optionId;
            Value = value;
        }


        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return SkuId;
            yield return VariantId;
            yield return OptionId;
            yield return Value;
        }
    }
}
