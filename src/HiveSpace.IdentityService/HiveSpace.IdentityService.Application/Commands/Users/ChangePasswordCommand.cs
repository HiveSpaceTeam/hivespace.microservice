using HiveSpace.Application.Shared.Commands;

namespace HiveSpace.IdentityService.Application.Commands.Users;

public record ChangePasswordCommand(
    string Password,
    string NewPassword
) : ICommand
{
    public static ChangePasswordCommand FromDto(Models.Requests.ChangePasswordRequestDto dto) =>
        new(
            dto.Password,
            dto.NewPassword
        );
}
