using HiveSpace.Domain.Shared.Enumerations;
using HiveSpace.UserService.Domain.Enums;

namespace HiveSpace.UserService.Application.Users.Dtos;

public record UpdateUserProfileRequestDto(
    string? FullName = null,
    string? UserName = null,
    string? PhoneNumber = null,
    Gender? Gender = null,
    DateTimeOffset? DateOfBirth = null,
    string? AvatarFileId = null
);
