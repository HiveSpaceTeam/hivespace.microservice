using HiveSpace.Core.Models.Filtering;
using HiveSpace.UserService.Domain.Enums;

namespace HiveSpace.UserService.Domain.Models;

public class AdminUserFilterRequest : FilterRequest
{
    public UserRoleFilter Role { get; set; } = UserRoleFilter.All;
    public UserStatusFilter Status { get; set; } = UserStatusFilter.All;
    public string? SearchTerm { get; set; }

    private static readonly string[] ValidSortFields =
    [
        "username", "fullname", "email", "status", "createddate", "lastlogindate"
    ];

    public override void Validate()
    {
        base.Validate();

        // Validate sort field
        var fieldLower = SortField.ToLowerInvariant();
        if (!ValidSortFields.Contains(fieldLower))
        {
            Sort = "createddate.desc";
        }

        // Validate sort direction
        var direction = SortDirection.ToLowerInvariant();
        if (direction != "asc" && direction != "desc")
        {
            Sort = $"{SortField}.desc";
        }
    }
}
