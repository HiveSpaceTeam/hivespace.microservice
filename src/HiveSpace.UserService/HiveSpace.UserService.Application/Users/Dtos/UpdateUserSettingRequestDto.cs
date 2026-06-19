namespace HiveSpace.UserService.Application.Users.Dtos;

public record UpdateUserSettingRequestDto(
    string? Theme = null,
    string? Culture = null
);
