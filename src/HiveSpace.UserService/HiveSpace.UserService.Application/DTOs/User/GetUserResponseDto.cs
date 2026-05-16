namespace HiveSpace.UserService.Application.DTOs.User;

public record GetUserSettingsResponseDto(
    string Theme,
    string Culture
);
