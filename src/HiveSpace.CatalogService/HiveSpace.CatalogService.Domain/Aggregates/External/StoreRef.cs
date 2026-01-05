using HiveSpace.Domain.Shared.Entities;

namespace HiveSpace.CatalogService.Domain.Aggregates.External
{
    public class StoreRef : AggregateRoot<Guid>
    {
        public Guid OwnerId { get; private set; }
        public string StoreName { get; private set; }
        public string? Description { get; private set; }
        public string LogoUrl { get; private set; }
        public string Address { get; private set; }
        public DateTimeOffset CreatedAt { get; private set; }
        public DateTimeOffset UpdatedAt { get; private set; }

        public StoreRef(Guid id, Guid ownerId, string storeName, string? description, string logoUrl, string address, DateTimeOffset createdAt, DateTimeOffset updatedAt)
        {
            Id = id;
            OwnerId = ownerId;
            StoreName = storeName;
            Description = description;
            LogoUrl = logoUrl;
            Address = address;
            CreatedAt = createdAt;
            UpdatedAt = updatedAt;
        }
    }
}
