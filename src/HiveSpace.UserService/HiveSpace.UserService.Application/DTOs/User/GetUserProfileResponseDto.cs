using HiveSpace.UserService.Domain.Enums;

namespace HiveSpace.UserService.Application.DTOs.User;

public record GetUserProfileResponseDto(
    string FullName,
    string UserName,
    string Email,
    string? PhoneNumber,
    Gender? Gender,
    DateTimeOffset? DateOfBirth
);
