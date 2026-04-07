using HiveSpace.UserService.Domain.Enums;

namespace HiveSpace.UserService.Application.DTOs.User;

public record UpdateUserProfileRequestDto(
    string? FullName = null,
    string? UserName = null,
    string? PhoneNumber = null,
    Gender? Gender = null,
    DateTimeOffset? DateOfBirth = null
);
