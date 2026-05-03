using HiveSpace.UserService.Application.Constant.Enum;

namespace HiveSpace.UserService.Application.DTOs.Admin;

public record SetUserStatusRequestDto(
    Guid UserId,
    bool IsActive,
    UserQueryType ResponseType
);