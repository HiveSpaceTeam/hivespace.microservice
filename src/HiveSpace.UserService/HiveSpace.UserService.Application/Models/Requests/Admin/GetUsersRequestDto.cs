using HiveSpace.UserService.Application.Constant.Enum;

namespace HiveSpace.UserService.Application.Models.Requests.Admin;

public record GetUsersRequestDto(
    int Page = 1,
    int PageSize = 10,
    UserRoleFilter Role = UserRoleFilter.All,
    UserStatusFilter Status = UserStatusFilter.All,
    string? SearchTerm = null,
    string Sort = "createdDate.desc"
);
