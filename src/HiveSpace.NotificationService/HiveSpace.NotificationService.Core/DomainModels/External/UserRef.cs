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

    /// <summary>BCP-47 locale, e.g. "vi", "en". Used to select notification template.</summary>
    public string         Locale      { get; private set; } = "vi";

    public DateTimeOffset UpdatedAt   { get; private set; }

    protected UserRef() { }

    public static UserRef Create(
        Guid id, string email, string fullName,
        string? phoneNumber = null, string locale = "vi",
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

    public void Update(string email, string fullName, string? phoneNumber, string locale,
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
