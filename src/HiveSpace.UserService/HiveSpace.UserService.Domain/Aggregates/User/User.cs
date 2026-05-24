using HiveSpace.Domain.Shared.Entities;
using HiveSpace.Domain.Shared.Enumerations;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.Domain.Shared.Interfaces;
using HiveSpace.UserService.Domain.Enums;
using HiveSpace.UserService.Domain.Exceptions;

namespace HiveSpace.UserService.Domain.Aggregates.User;

public class User : AggregateRoot<Guid>, IAuditable, ISoftDeletable
{
    public Email Email { get; private set; }
    public string UserName { get; private set; }
    public string FullName { get; private set; }
    public string? AvatarFileId { get; private set; }
    public string? AvatarUrl { get; private set; }
    public PhoneNumber? PhoneNumber { get; private set; }
    public DateOfBirth? DateOfBirth { get; private set; }
    public Gender? Gender { get; private set; }

    private readonly List<Address> _addresses;
    public IReadOnlyCollection<Address> Addresses => _addresses.AsReadOnly();

    public UserSettings Settings { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }

    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }

    private User()
    {
        _addresses = [];
        Email = null!;
        UserName = string.Empty;
        FullName = string.Empty;
        Settings = new UserSettings(Theme.Light, Culture.En);
    }

    private User(
        Email email,
        string userName,
        string fullName,
        string? avatarUrl = null,
        PhoneNumber? phoneNumber = null,
        DateOfBirth? dateOfBirth = null,
        Gender? gender = null,
        DateTimeOffset? createdAt = null,
        DateTimeOffset? updatedAt = null)
    {
        _addresses = [];
        Email = email;
        UserName = userName.Trim();
        FullName = fullName.Trim();
        AvatarUrl = avatarUrl?.Trim();
        PhoneNumber = phoneNumber;
        DateOfBirth = dateOfBirth;
        Gender = gender;
        CreatedAt = createdAt ?? DateTimeOffset.UtcNow;
        UpdatedAt = updatedAt;
        Settings = new UserSettings(Theme.Light, Culture.En);
    }

    internal static User Rehydrate(
        Guid id,
        Email email,
        string userName,
        string fullName,
        string? avatarUrl,
        PhoneNumber? phoneNumber,
        DateOfBirth? dateOfBirth,
        Gender? gender,
        DateTimeOffset createdAt,
        DateTimeOffset? updatedAt,
        bool isDeleted = false,
        DateTimeOffset? deletedAt = null,
        IEnumerable<Address>? addresses = null,
        Theme theme = Theme.Light,
        Culture culture = Culture.Vi,
        string? avatarFileId = null)
    {
        var user = new User(email, userName, fullName, avatarUrl, phoneNumber, dateOfBirth, gender, createdAt, updatedAt)
        {
            Id = id,
            AvatarFileId = avatarFileId,
            IsDeleted = isDeleted,
            DeletedAt = deletedAt,
            Settings = new UserSettings(theme, culture)
        };

        if (addresses != null)
        {
            foreach (var address in addresses)
            {
                user._addresses.Add(address);
            }
        }

        return user;
    }

    public static User CreateProfile(
        Guid id,
        Email email,
        string userName,
        string fullName,
        string? avatarUrl = null,
        PhoneNumber? phoneNumber = null,
        DateOfBirth? dateOfBirth = null,
        Gender? gender = null,
        DateTimeOffset? createdAt = null)
    {
        ValidateProfileAndThrow(email, userName, fullName);

        var user = new User(email, userName, fullName, avatarUrl, phoneNumber, dateOfBirth, gender, createdAt);
        user.Id = id;

        return user;
    }

    private static void ValidateProfileAndThrow(Email? email, string? userName, string? fullName)
    {
        if (email == null)
            throw new InvalidUserInformationException();
        if (string.IsNullOrWhiteSpace(userName))
            throw new InvalidUserInformationException();
        if (string.IsNullOrWhiteSpace(fullName))
            throw new InvalidUserInformationException();

        var trimmedUserName = userName.Trim();
        var trimmedFullName = fullName.Trim();

        if (trimmedUserName.Length < 3 || trimmedUserName.Length > 50)
            throw new InvalidUserInformationException();
        if (trimmedFullName.Length < 2 || trimmedFullName.Length > 100)
            throw new InvalidUserInformationException();
        if (ContainsInvalidUsernameCharacters(trimmedUserName))
            throw new InvalidUserInformationException();
    }

    private static bool ContainsInvalidUsernameCharacters(string userName)
        => !userName.All(c => char.IsLetterOrDigit(c) || c == '_' || c == '-' || c == '@' || c == '.');

    public void SetAvatar(string fileId)
    {
        if (string.IsNullOrWhiteSpace(fileId))
            throw new InvalidFieldException(UserDomainErrorCode.InvalidField, nameof(AvatarFileId));

        AvatarFileId = fileId.Trim();
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void SetAvatarUrl(string url)
    {
        AvatarUrl = url;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void UpdateProfile(string? fullName, PhoneNumber? phoneNumber, DateOfBirth? dateOfBirth, Gender? gender, string? userName = null)
    {
        if (!string.IsNullOrWhiteSpace(fullName))
            FullName = fullName.Trim();

        if (phoneNumber != null)
            PhoneNumber = phoneNumber;

        if (dateOfBirth != null)
            DateOfBirth = dateOfBirth;

        if (gender != null)
            Gender = gender;

        if (!string.IsNullOrWhiteSpace(userName))
        {
            var trimmed = userName.Trim();
            if (trimmed.Length < 3 || trimmed.Length > 50 || ContainsInvalidUsernameCharacters(trimmed))
                throw new InvalidFieldException(UserDomainErrorCode.InvalidUserInformation, nameof(UserName));

            UserName = trimmed;
        }

        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public Address AddAddress(
        string fullName,
        string phoneNumber,
        string street,
        string commune,
        string province,
        string country,
        string? zipCode,
        AddressType addressType,
        bool setAsDefault = false)
    {
        var address = new Address(fullName, phoneNumber, street, commune, province, country, zipCode, addressType);

        if (setAsDefault)
        {
            foreach (var existingAddress in _addresses)
            {
                existingAddress.RemoveDefaultStatus();
            }

            address.SetAsDefault();
        }

        _addresses.Add(address);
        UpdatedAt = DateTimeOffset.UtcNow;

        return address;
    }

    public void UpdateAddress(
        Guid addressId,
        string? fullName,
        string? phoneNumber,
        string? street,
        string? commune,
        string? province,
        string? country,
        string? zipCode,
        AddressType? addressType)
    {
        var address = _addresses.FirstOrDefault(a => a.Id == addressId)
            ?? throw new NotFoundException(UserDomainErrorCode.AddressNotFound, nameof(Address));

        address.UpdateDetails(fullName, phoneNumber, street, commune, province, country, zipCode, addressType);
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void RemoveAddress(Guid addressId)
    {
        var address = _addresses.FirstOrDefault(a => a.Id == addressId)
            ?? throw new NotFoundException(UserDomainErrorCode.AddressNotFound, nameof(Address));

        if (_addresses.Count == 1)
            throw new CannotRemoveOnlyAddressException();

        if (address.IsDefault)
            throw new CannotRemoveDefaultAddressException();

        _addresses.Remove(address);
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void MarkAddressAsDefault(Guid addressId)
    {
        var targetAddress = _addresses.FirstOrDefault(a => a.Id == addressId)
            ?? throw new NotFoundException(UserDomainErrorCode.AddressNotFound, nameof(Address));

        foreach (var address in _addresses)
        {
            address.RemoveDefaultStatus();
        }

        targetAddress.SetAsDefault();
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void UpdateTheme(Theme theme)
    {
        Settings = Settings.WithTheme(theme);
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void UpdateCulture(Culture culture)
    {
        Settings = Settings.WithCulture(culture);
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
