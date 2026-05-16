using HiveSpace.Domain.Shared.Enumerations;

namespace HiveSpace.NotificationService.Core.DomainModels.External;

public class UserRef
{
    public Guid           Id          { get; private set; }
    public string         Email       { get; private set; } = default!;
    public string?        PhoneNumber { get; private set; }
    public string         FullName    { get; private set; } = default!;
    public string?        UserName    { get; private set; }
    public string?        AvatarUrl   { get; private set; }

    // Populated for seller accounts via StoreCreatedIntegrationEvent
    public Guid?          StoreId      { get; private set; }
    public string?        StoreName    { get; private set; }
    public string?        StoreLogoUrl { get; private set; }

    public Culture        Locale      { get; private set; } = Culture.Vi;

    public DateTimeOffset UpdatedAt   { get; private set; }

    protected UserRef() { }

    public static UserRef Create(
        Guid id, string email, string fullName,
        string? phoneNumber = null, Culture locale = Culture.Vi,
        string? userName = null, string? avatarUrl = null,
        Guid? storeId = null, string? storeName = null, string? storeLogoUrl = null)
        => new()
        {
            Id           = id,
            Email        = email,
            FullName     = fullName,
            PhoneNumber  = phoneNumber,
            Locale       = locale,
            UserName     = userName,
            AvatarUrl    = avatarUrl,
            StoreId      = storeId,
            StoreName    = storeName,
            StoreLogoUrl = storeLogoUrl,
            UpdatedAt    = DateTimeOffset.UtcNow,
        };

    public void Update(string email, string fullName, string? phoneNumber, Culture locale,
        string? userName = null, string? avatarUrl = null)
    {
        Email       = email;
        FullName    = fullName;
        PhoneNumber = phoneNumber;
        Locale      = locale;
        UserName    = userName;
        AvatarUrl   = avatarUrl;
        UpdatedAt   = DateTimeOffset.UtcNow;
    }

    public void UpdateStore(Guid storeId, string storeName, string? storeLogoUrl)
    {
        StoreId      = storeId;
        StoreName    = storeName;
        StoreLogoUrl = storeLogoUrl;
        UpdatedAt    = DateTimeOffset.UtcNow;
    }
}
