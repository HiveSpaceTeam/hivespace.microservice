using HiveSpace.Domain.Shared.Entities;

namespace HiveSpace.CatalogService.Domain.Aggregates.ProductAggregate
{
    public class SkuImage : ValueObject
    {
        public string FileId { get; private set; }

        public SkuImage( string fileId)
        {
            FileId = fileId;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return FileId;
        }
    }
}
