using HiveSpace.Domain.Shared.Entities;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.UserService.Domain.Enums;
using HiveSpace.UserService.Domain.Exceptions;


namespace HiveSpace.UserService.Domain.Aggregates.Store;

public class Store : AggregateRoot<Guid>
{
    public Guid OwnerId { get; private set; }
    public string StoreName { get; private set; }
    public string? Description { get; private set; }
    public string LogoUrl { get; private set; }
    public string Address { get; private set; }
    public StoreStatus Status { get; private set; }
    
    // Audit
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    private Store()
    {
        
    }
    
    // Internal factory method for domain service use only
    internal static Store Create(string storeName, string? description, string logoUrl, string storeAddress, Guid ownerId)
    {
        ValidateAndThrow(storeName, description, logoUrl, storeAddress, ownerId);
        
        return new Store
        {
            Id = Guid.NewGuid(),
            StoreName = storeName.Trim(),
            Description = description?.Trim(),
            LogoUrl = logoUrl.Trim(),
            Address = storeAddress.Trim(),
            OwnerId = ownerId,
            Status = StoreStatus.Active,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }
    
    private static void ValidateAndThrow(string? storeName, string? description, string? logoUrl, string? storeAddress, Guid ownerId)
    {
        if (string.IsNullOrWhiteSpace(storeName))
            throw new InvalidFieldException(UserDomainErrorCode.InvalidField, nameof(Store.StoreName));
        if (string.IsNullOrWhiteSpace(logoUrl))
            throw new InvalidFieldException(UserDomainErrorCode.InvalidField, nameof(Store.LogoUrl));
        if (string.IsNullOrWhiteSpace(storeAddress))
            throw new InvalidFieldException(UserDomainErrorCode.InvalidField, nameof(Store.Address));
        if (ownerId == Guid.Empty)
            throw new InvalidFieldException(UserDomainErrorCode.InvalidField, nameof(Store.OwnerId));
            
        ValidateStoreName(storeName);
        ValidateStoreDescription(description);
        ValidateLogoUrl(logoUrl);
        ValidateStoreAddress(storeAddress);
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
                throw new InvalidFieldException(UserDomainErrorCode.InvalidField, nameof(Store.Description));
        }
    }
    
    private static void ValidateLogoUrl(string? logoUrl)
    {
        if (string.IsNullOrEmpty(logoUrl))
            throw new InvalidFieldException(UserDomainErrorCode.InvalidField, nameof(Store.LogoUrl));
            
        var trimmedLogoUrl = logoUrl.Trim();
        if (trimmedLogoUrl.Length > 500)
            throw new InvalidFieldException(UserDomainErrorCode.InvalidField, nameof(Store.LogoUrl));
    }
    
    private static void ValidateStoreAddress(string? storeAddress)
    {
        if (string.IsNullOrEmpty(storeAddress))
            throw new InvalidFieldException(UserDomainErrorCode.InvalidField, nameof(Store.Address));
            
        var trimmedAddress = storeAddress.Trim();
        if (trimmedAddress.Length > 500)
            throw new InvalidFieldException(UserDomainErrorCode.InvalidField, nameof(Store.Address));
    }
    
    // Domain Methods
    public void UpdateDetails(string? storeName, string? storeDescription, string? logoUrl, string? storeAddress)
    {
        if (!string.IsNullOrWhiteSpace(storeName))
        {
            ValidateStoreName(storeName);
            StoreName = storeName.Trim();
        }
        
        if (storeDescription != null)
        {
            ValidateStoreDescription(storeDescription);
            Description = storeDescription.Trim();
        }
        
        if (!string.IsNullOrWhiteSpace(logoUrl))
        {
            ValidateLogoUrl(logoUrl);
            LogoUrl = logoUrl.Trim();
        }
        
        if (!string.IsNullOrWhiteSpace(storeAddress))
        {
            ValidateStoreAddress(storeAddress);
            Address = storeAddress.Trim();
        }
    }
    
    public void UpdateLogo(string logoUrl)
    {
        ValidateLogoUrl(logoUrl);
        LogoUrl = string.IsNullOrWhiteSpace(logoUrl) ? string.Empty : logoUrl.Trim();
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
