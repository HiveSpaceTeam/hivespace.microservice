using HiveSpace.UserService.Application.Constant.Enum;

namespace HiveSpace.UserService.Application.Models.Requests.Admin;

public record SetUserStatusRequestDto(
    Guid UserId,
    bool IsActive,
    UserQueryType ResponseType
);