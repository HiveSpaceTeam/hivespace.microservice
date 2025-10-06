namespace HiveSpace.UserService.Application.Models.Requests.Admin;

public record SetUserStatusRequestDto(
    Guid UserId,
    bool IsActive
);


