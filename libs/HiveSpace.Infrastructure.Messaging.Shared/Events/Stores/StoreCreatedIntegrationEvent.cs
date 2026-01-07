
using HiveSpace.Infrastructure.Messaging.Events;

namespace HiveSpace.Infrastructure.Messaging.Shared.Events.Stores;

public record StoreCreatedIntegrationEvent : IntegrationEvent
{
    public StoreCreatedIntegrationEvent(Guid id, Guid ownerId, string storeName, string? description, string logoUrl, string address)
    {
        Id = id;
        OwnerId = ownerId;
        StoreName = storeName;
        Description = description;
        LogoUrl = logoUrl;
        Address = address;
    }

    public Guid Id { get; private set; }
    public Guid OwnerId { get; private set; }
    public string StoreName { get; private set; }
    public string? Description { get; private set; }
    public string LogoUrl { get; private set; }
    public string Address { get; private set; }
}

