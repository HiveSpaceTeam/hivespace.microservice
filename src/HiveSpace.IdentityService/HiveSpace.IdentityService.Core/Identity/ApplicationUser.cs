using Microsoft.AspNetCore.Identity;

namespace HiveSpace.IdentityService.Core.Identity;

public class ApplicationUser : IdentityUser<Guid>
{
    public int Status { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public DateTimeOffset? LastLoginAt { get; set; }

    public string? RoleName { get; set; }
    public Guid? StoreId { get; set; }
}
