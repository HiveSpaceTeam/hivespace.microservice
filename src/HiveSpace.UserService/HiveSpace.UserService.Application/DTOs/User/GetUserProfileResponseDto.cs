using HiveSpace.UserService.Domain.Enums;

namespace HiveSpace.UserService.Application.DTOs.User;

public record GetUserProfileResponseDto(
    string FullName,
    string UserName,
    string Email,
    string? AvatarUrl,
    string? PhoneNumber,
    Gender? Gender,
    DateTimeOffset? DateOfBirth
);
