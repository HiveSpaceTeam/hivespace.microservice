using System;
using HiveSpace.Core.Models.Filtering;
using HiveSpace.UserService.Application.Constant.Enum;

namespace HiveSpace.UserService.Application.Models.Queries;

using System.Collections.Generic;

public class AdminUserFilterRequest : FilterRequest
{
    public RoleFilter Role { get; set; } = RoleFilter.All;
    public StatusFilter Status { get; set; } = StatusFilter.All;
    public string? SearchTerm { get; set; }

    private static readonly HashSet<string> ValidSortFields = new(StringComparer.OrdinalIgnoreCase)
    {
        "username", "fullname", "email", "status", "createdDate", "lastLoginDate"
    };

    public override void Validate()
    {
        base.Validate();

        // Coerce out-of-range enum values to 'All'
        if (!Enum.IsDefined(typeof(RoleFilter), Role))
        {
            Role = RoleFilter.All;
        }

        if (!Enum.IsDefined(typeof(StatusFilter), Status))
        {
            Status = StatusFilter.All;
        }

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
