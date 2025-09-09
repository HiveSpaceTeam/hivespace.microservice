namespace HiveSpace.UserService.Application.Models.Responses;

public record CreateAdminResponseDto(
    Guid Id,
    string Email,
    string UserName,
    string FullName,
    string Role
);
