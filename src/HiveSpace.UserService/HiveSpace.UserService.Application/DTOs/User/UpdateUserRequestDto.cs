namespace HiveSpace.UserService.Application.DTOs.User;

public record UpdateUserSettingRequestDto(
    string? Theme = null,
    string? Culture = null
);
