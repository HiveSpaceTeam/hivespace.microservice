namespace HiveSpace.CatalogService.Domain.Aggregates.External
{
    public class StoreRef
    {
        public Guid Id { get;  set; }
        public Guid OwnerId { get;  set; }
        public string StoreName { get;  set; }
        public string? Description { get;  set; }
        public string LogoUrl { get;  set; }
        public string Address { get;  set; }
        public DateTimeOffset CreatedAt { get;  set; }
        public DateTimeOffset UpdatedAt { get;  set; }
    }
}
