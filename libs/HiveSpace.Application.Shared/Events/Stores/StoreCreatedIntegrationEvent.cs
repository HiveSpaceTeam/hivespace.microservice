
using HiveSpace.Infrastructure.Messaging.Events;

namespace HiveSpace.Application.Shared.Events.Stores;

public record StoreCreatedIntegrationEvent : IntegrationEvent
{
    public StoreCreatedIntegrationEvent(Guid ownerId, string storeName, string? description, string logoUrl, string address)
    {
        OwnerId = ownerId;
        StoreName = storeName;
        Description = description;
        LogoUrl = logoUrl;
        Address = address;
    }

    public Guid OwnerId { get; private set; }
    public string StoreName { get; private set; }
    public string? Description { get; private set; }
    public string LogoUrl { get; private set; }
    public string Address { get; private set; }
}

