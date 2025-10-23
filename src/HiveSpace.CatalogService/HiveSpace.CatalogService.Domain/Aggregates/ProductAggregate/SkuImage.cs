using HiveSpace.Domain.Shared.Entities;

namespace HiveSpace.CatalogService.Domain.Aggregates.ProductAggregate
{
    public class SkuImage : ValueObject
    {
        public Guid SkuId { get; private set; }
        public string FileId { get; private set; }

        public SkuImage(Guid skuId, string fileId)
        {
            SkuId = skuId;
            FileId = fileId;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return SkuId;
            yield return FileId;
        }
    }
}
