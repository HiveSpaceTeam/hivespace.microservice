using HiveSpace.Domain.Shared.Entities;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.Domain.Shared.Enumerations;
using HiveSpace.UserService.Domain.Enums;
using HiveSpace.UserService.Domain.Exceptions;


namespace HiveSpace.UserService.Domain.Aggregates.Store;

public class Store : AggregateRoot<Guid>
{
    public Guid OwnerId { get; private set; }
    public string StoreName { get; private set; }
    public string? Description { get; private set; }
    public string LogoFileId { get; private set; }
    public string? LogoUrl { get; private set; }
    public string Address { get; private set; }
    public StoreStatus Status { get; private set; }

    // Audit
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    private Store()
    {
        LogoFileId = string.Empty;
    }

    internal static Store Create(string storeName, string? description, string logoFileId, string storeAddress, Guid ownerId, Guid? storeId)
    {
        ValidateAndThrow(storeName, description, logoFileId, storeAddress, ownerId);

        return new Store
        {
            Id = storeId ?? Guid.NewGuid(),
            StoreName = storeName.Trim(),
            Description = description?.Trim(),
            LogoFileId = logoFileId.Trim(),
            LogoUrl = null,
            Address = storeAddress.Trim(),
            OwnerId = ownerId,
            Status = StoreStatus.Active,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
    }

    private static void ValidateAndThrow(string? storeName, string? description, string? logoFileId, string? storeAddress, Guid ownerId)
    {
        if (string.IsNullOrWhiteSpace(storeName))
            throw new InvalidFieldException(UserDomainErrorCode.InvalidField, nameof(Store.StoreName));
        if (string.IsNullOrWhiteSpace(logoFileId))
            throw new InvalidFieldException(UserDomainErrorCode.InvalidField, nameof(Store.LogoFileId));
        if (string.IsNullOrWhiteSpace(storeAddress))
            throw new InvalidFieldException(UserDomainErrorCode.InvalidField, nameof(Store.Address));
        if (ownerId == Guid.Empty)
            throw new InvalidFieldException(UserDomainErrorCode.InvalidField, nameof(Store.OwnerId));

        ValidateStoreName(storeName);
        ValidateStoreDescription(description);
        ValidateLogoFileId(logoFileId);
        ValidateStoreAddress(storeAddress);
    }

    private static void ValidateStoreName(string storeName)
    {
        var trimmed = storeName.Trim();
        if (trimmed.Length < 2 || trimmed.Length > 100)
            throw new InvalidFieldException(UserDomainErrorCode.InvalidField, nameof(Store.StoreName));
    }

    private static void ValidateStoreDescription(string? description)
    {
        if (!string.IsNullOrEmpty(description) && description.Trim().Length > 500)
            throw new InvalidFieldException(UserDomainErrorCode.InvalidField, nameof(Store.Description));
    }

    private static void ValidateLogoFileId(string? logoFileId)
    {
        if (string.IsNullOrEmpty(logoFileId))
            throw new InvalidFieldException(UserDomainErrorCode.InvalidField, nameof(Store.LogoFileId));
        if (logoFileId.Trim().Length > 100)
            throw new InvalidFieldException(UserDomainErrorCode.InvalidField, nameof(Store.LogoFileId));
    }

    private static void ValidateStoreAddress(string? storeAddress)
    {
        if (string.IsNullOrEmpty(storeAddress) || storeAddress.Trim().Length > 500)
            throw new InvalidFieldException(UserDomainErrorCode.InvalidField, nameof(Store.Address));
    }

    public void UpdateDetails(string? storeName, string? storeDescription, string? logoFileId, string? storeAddress)
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

        if (!string.IsNullOrWhiteSpace(logoFileId))
        {
            ValidateLogoFileId(logoFileId);
            LogoFileId = logoFileId.Trim();
            LogoUrl = null;
        }

        if (!string.IsNullOrWhiteSpace(storeAddress))
        {
            ValidateStoreAddress(storeAddress);
            Address = storeAddress.Trim();
        }
    }

    public void UpdateLogo(string logoFileId)
    {
        ValidateLogoFileId(logoFileId);
        LogoFileId = logoFileId.Trim();
        LogoUrl = null;
    }

    public void SetLogoUrl(string url)
    {
        LogoUrl = url;
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
