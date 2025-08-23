using HiveSpace.Domain.Shared.Entities;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.UserService.Domain.Aggregates.User;
using HiveSpace.UserService.Domain.Enums;
using HiveSpace.UserService.Domain.Exceptions;


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
        ValidateAndThrow(storeName, description, ownerId);
        
        return new Store
        {
            StoreName = storeName.Trim(),
            StoreDescription = description?.Trim(),
            ContactPhone = contactPhone,
            OwnerId = ownerId,
            Status = StoreStatus.Active,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }
    
    private static void ValidateAndThrow(string? storeName, string? description, Guid ownerId)
    {
        if (string.IsNullOrWhiteSpace(storeName))
            throw new InvalidFieldException(UserDomainErrorCode.InvalidField, nameof(Store.StoreName));
        if (ownerId == Guid.Empty)
            throw new InvalidFieldException(UserDomainErrorCode.InvalidField, nameof(Store.OwnerId));
            
        ValidateStoreName(storeName);
        ValidateStoreDescription(description);
    }
    
    private static void ValidateStoreName(string storeName)
    {
        var trimmedStoreName = storeName.Trim();
        if (trimmedStoreName.Length < 2 || trimmedStoreName.Length > 100)
            throw new InvalidFieldException(UserDomainErrorCode.InvalidField, nameof(Store.StoreName));
    }
    
    private static void ValidateStoreDescription(string? description)
    {
        if (!string.IsNullOrEmpty(description))
        {
            var trimmedDescription = description.Trim();
            if (trimmedDescription.Length > 500)
                throw new InvalidFieldException(UserDomainErrorCode.InvalidField, nameof(Store.StoreDescription));
        }
    }
    
    private static void ValidateLogoUrl(string? logoUrl)
    {
        if (!string.IsNullOrEmpty(logoUrl))
        {
            var trimmedLogoUrl = logoUrl.Trim();
            if (trimmedLogoUrl.Length > 500)
                throw new InvalidFieldException(UserDomainErrorCode.InvalidField, nameof(Store.LogoUrl));
        }
    }
    
    // Domain Methods
    public void UpdateDetails(string? storeName, string? storeDescription, PhoneNumber? contactPhone)
    {
        if (!string.IsNullOrWhiteSpace(storeName))
        {
            ValidateStoreName(storeName);
            StoreName = storeName.Trim();
        }
        
        if (storeDescription != null)
        {
            ValidateStoreDescription(storeDescription);
            StoreDescription = storeDescription.Trim();
        }
        
        if (contactPhone != null) ContactPhone = contactPhone;
    }
    
    public void UpdateLogo(string logoUrl)
    {
        ValidateLogoUrl(logoUrl);
        LogoUrl = string.IsNullOrWhiteSpace(logoUrl) ? null : logoUrl.Trim();
    }
    
    public void Activate()
    {
        Status = StoreStatus.Active;
    }
    
    public void Deactivate()
    {
        Status = StoreStatus.Inactive;
    }
}
