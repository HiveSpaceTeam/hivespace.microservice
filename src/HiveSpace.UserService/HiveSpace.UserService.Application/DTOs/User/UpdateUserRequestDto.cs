using HiveSpace.UserService.Domain.Enums;

namespace HiveSpace.UserService.Application.DTOs.User;

public record UpdateUserSettingRequestDto(
    Theme? Theme = null,
    Culture? Culture = null
);