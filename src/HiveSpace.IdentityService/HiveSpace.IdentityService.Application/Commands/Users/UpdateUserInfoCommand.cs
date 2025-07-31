using HiveSpace.Application.Shared.Commands;
using HiveSpace.IdentityService.Application.Models.Requests;
using HiveSpace.IdentityService.Domain.Aggregates.Enums;

namespace HiveSpace.IdentityService.Application.Commands.Users;

public record UpdateUserInfoCommand(
    string? UserName,
    string? FullName,
    string? Email,
    string? PhoneNumber,
    Gender? Gender,
    DateTimeOffset? DateOfBirth
) : ICommand
{
    public static UpdateUserInfoCommand FromDto(UpdateUserRequestDto dto) =>
        new(
            dto.UserName,
            dto.FullName,
            dto.Email,
            dto.PhoneNumber,
            dto.Gender,
            dto.DateOfBirth
        );
}
