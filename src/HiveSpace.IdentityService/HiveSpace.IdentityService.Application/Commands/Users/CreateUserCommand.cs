using HiveSpace.Application.Shared.Commands;

namespace HiveSpace.IdentityService.Application.Commands.Users;

public record CreateUserCommand(
    string Email,
    string UserName,
    string FullName,
    string Password
) : ICommand<Models.Responses.SignupResponseDto>
{
    public static CreateUserCommand FromDto(Models.Requests.SignupRequestDto dto) =>
        new(
            dto.Email,
            dto.UserName,
            dto.FullName,
            dto.Password
        );
}
