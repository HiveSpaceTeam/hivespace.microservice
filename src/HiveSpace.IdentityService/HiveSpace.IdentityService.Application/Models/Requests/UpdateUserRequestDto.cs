using HiveSpace.IdentityService.Domain.Aggregates.Enums;

namespace HiveSpace.IdentityService.Application.Models.Requests;

public record UpdateUserRequestDto(
    string? FullName,
    string? UserName,
    string? Email,
    string? PhoneNumber,
    Gender? Gender,
    DateTimeOffset? DateOfBirth
);
