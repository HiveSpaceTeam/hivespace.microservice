using Microsoft.AspNetCore.Identity;
using HiveSpace.UserService.Domain.Aggregates.User;
using HiveSpace.UserService.Domain.Enums;

namespace HiveSpace.UserService.Infrastructure.Identity;

public class ApplicationUser : IdentityUser<Guid>
{
    // Additional fields from User domain entity that are not in IdentityUser
    public string FullName { get; set; } = string.Empty;
    public Guid? StoreId { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public int? Gender { get; set; }
    public int Status { get; set; } = (int)UserStatus.Active;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public DateTimeOffset? LastLoginAt { get; set; }
    
    // Navigation properties - using domain Address entity
    public virtual ICollection<Address> Addresses { get; set; } = new List<Address>();
    
    // Navigation property for user roles
    public virtual ICollection<IdentityUserRole<Guid>> UserRoles { get; set; } = new List<IdentityUserRole<Guid>>();
}