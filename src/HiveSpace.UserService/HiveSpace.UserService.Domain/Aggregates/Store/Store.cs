using HiveSpace.Domain.Shared.Entities;
using HiveSpace.UserService.Domain.Aggregates.User;
using HiveSpace.UserService.Domain.Enums;


namespace HiveSpace.UserService.Domain.Aggregates.Store;

public class Store : AggregateRoot<Guid>
{
    public Guid OwnerId { get; private set; }
    public string StoreName { get; private set; }
    public string? StoreDescription { get; private set; }
    public string? LogoUrl { get; private set; }
    public PhoneNumber? ContactPhone { get; private set; }
    public StoreStatus Status { get; private set; }
    
    // Audit
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    
    private Store() 
    {
    }
    
    // Internal factory method for domain service use only
    internal static Store Create(string storeName, string? description, PhoneNumber? contactPhone, Guid ownerId)
    {
        return new Store
        {
            Id = Guid.NewGuid(),
            StoreName = storeName,
            StoreDescription = description,
            ContactPhone = contactPhone,
            OwnerId = ownerId,
            Status = StoreStatus.Active,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
    }
    
    // Domain Methods
    public void UpdateDetails(string? storeName, string? storeDescription, PhoneNumber? contactPhone)
    {
        if (!string.IsNullOrWhiteSpace(storeName)) StoreName = storeName;
        if (storeDescription != null) StoreDescription = storeDescription;
        if (contactPhone != null) ContactPhone = contactPhone;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
    
    public void UpdateLogo(string logoUrl)
    {
        LogoUrl = logoUrl;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
    
    public void Activate()
    {
        Status = StoreStatus.Active;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
    
    public void Deactivate()
    {
        Status = StoreStatus.Inactive;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
