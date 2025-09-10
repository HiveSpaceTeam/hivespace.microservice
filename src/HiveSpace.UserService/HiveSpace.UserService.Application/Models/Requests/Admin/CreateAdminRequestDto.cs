namespace HiveSpace.UserService.Application.Models.Requests.Admin;

public record CreateAdminRequestDto(
    string Email,
    string Password,
    string FullName,
    string ConfirmPassword,
    bool IsSystemAdmin = false // "Admin" or "SystemAdmin"
);
