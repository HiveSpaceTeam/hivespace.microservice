using HiveSpace.UserService.Domain.Enums;

namespace HiveSpace.UserService.Application.Models.Requests.User;

public record UpdateUserSettingRequestDto(
    Theme? Theme = null,
    Culture? Culture = null
);