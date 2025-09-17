using HiveSpace.Core.Models.Filtering;
using HiveSpace.UserService.Application.Constant.Enum;

namespace HiveSpace.UserService.Application.Models.Queries;

using System.Collections.Generic;

public class AdminUserFilterRequest : FilterRequest
{
    public UserRoleFilter Role { get; set; } = UserRoleFilter.All;
    public UserStatusFilter Status { get; set; } = UserStatusFilter.All;
    public string? SearchTerm { get; set; }

    private static readonly HashSet<string> ValidSortFields = new(StringComparer.OrdinalIgnoreCase)
    {
        "username", "fullname", "email", "status", "createdDate", "lastLoginDate"
    };

    public override void Validate()
    {
        base.Validate();

        // Validate sort field
        if (!ValidSortFields.Contains(SortField))
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
