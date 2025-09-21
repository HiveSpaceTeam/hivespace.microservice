using HiveSpace.UserService.Application.Constant.Enum;

namespace HiveSpace.UserService.Application.Models.Requests.Admin;

public record GetUsersRequestDto(
    int Page = 1,
    int PageSize = 10,
    int Role = (int)RoleFilter.All,
    int Status = (int)StatusFilter.All,
    string? SearchTerm = null,
    string Sort = "createdAt.desc"
);
