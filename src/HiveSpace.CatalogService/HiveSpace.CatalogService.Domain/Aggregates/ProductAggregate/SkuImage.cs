using HiveSpace.Domain.Shared.Entities;

namespace HiveSpace.CatalogService.Domain.Aggregates.ProductAggregate
{
    public class SkuImage : ValueObject
    {
        public string SkuId { get; private set; }
        public string FileId { get; private set; }

        public SkuImage(string productId, string fileId)
        {
            SkuId = productId;
            FileId = fileId;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return SkuId;
            yield return FileId;
        }
    }
}
