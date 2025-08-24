using Microsoft.AspNetCore.Identity;
using HiveSpace.UserService.Domain.Aggregates.User;

namespace HiveSpace.UserService.Infrastructure.Identity;

public class ApplicationUser : IdentityUser
{
    // Additional fields from User domain entity that are not in IdentityUser
    public string FullName { get; set; } = string.Empty;
    public Guid? StoreId { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public string UserStatus { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public DateTimeOffset? LastLoginAt { get; set; }
    
    // Navigation properties - using domain Address entity
    public virtual ICollection<Address> Addresses { get; set; } = new List<Address>();
}