namespace HiveSpace.UserService.Application.Users.Dtos;

public record GetUserSettingsResponseDto(
    string Theme,
    string Culture
);
