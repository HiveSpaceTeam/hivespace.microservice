using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.Domain.Shared.Interfaces;
using HiveSpace.UserService.Domain.Aggregates.User;
using HiveSpace.Domain.Shared.Enumerations;
using HiveSpace.UserService.Domain.Enums;
using HiveSpace.UserService.Domain.Exceptions;
using Microsoft.AspNetCore.Identity;

namespace HiveSpace.UserService.Infrastructure.Identity;

public class ApplicationUser : IdentityUser<Guid>, ISoftDeletable
{
    // Additional fields from User domain entity that are not in IdentityUser
    public string FullName { get; set; } = string.Empty;
    public string? AvatarFileId { get; set; }
    public string? AvatarUrl { get; set; }
    public Guid? StoreId { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public int? Gender { get; set; }
    public int Status { get; set; } = (int)UserStatus.Active;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public DateTimeOffset? LastLoginAt { get; set; }
    
    // Role stored directly instead of using IdentityUserRole relationship
    public string? RoleName { get; set; }
    
    // Navigation properties
    public virtual ICollection<Address> Addresses { get; set; } = new List<Address>();
    // Settings as primitive values since UserSettings is now a ValueObject
    public Theme Theme { get; set; } = Theme.Light;
    public Culture Culture { get; set; } = Culture.Vi;

    // ISoftDeletable
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }

    public void SetAvatarUrl(string url)
    {
        if (string.IsNullOrEmpty(AvatarFileId))
            throw new InvalidFieldException(UserDomainErrorCode.InvalidField, nameof(AvatarFileId));
        AvatarUrl = url;
    }
}
