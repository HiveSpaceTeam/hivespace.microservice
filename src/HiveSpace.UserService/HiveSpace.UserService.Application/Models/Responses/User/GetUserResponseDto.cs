using HiveSpace.UserService.Domain.Enums;

namespace HiveSpace.UserService.Application.Models.Responses.User;

public record GetUserSettingsResponseDto(
    Theme Theme,
    Culture Culture
);