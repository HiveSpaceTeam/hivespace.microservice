using HiveSpace.UserService.Domain.Enums;

namespace HiveSpace.UserService.Application.DTOs.User;

public record GetUserSettingsResponseDto(
    Theme Theme,
    Culture Culture
);