namespace HiveSpace.UserService.Application.DTOs.Admin;

public record CreateAdminRequestDto(
    string Email,
    string Password,
    string FullName,
    string ConfirmPassword,
    bool IsSystemAdmin = false
);
