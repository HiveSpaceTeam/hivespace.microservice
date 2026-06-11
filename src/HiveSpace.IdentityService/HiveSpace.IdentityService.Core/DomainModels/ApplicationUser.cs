using Microsoft.AspNetCore.Identity;

namespace HiveSpace.IdentityService.Core.DomainModels;

public class ApplicationUser : IdentityUser<Guid>
{
    public string? FullName { get; set; }
    public UserStatus Status { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? ActivatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public DateTimeOffset? LastLoginAt { get; set; }

    public string? RoleName { get; set; }
    public Guid? StoreId { get; set; }
}
