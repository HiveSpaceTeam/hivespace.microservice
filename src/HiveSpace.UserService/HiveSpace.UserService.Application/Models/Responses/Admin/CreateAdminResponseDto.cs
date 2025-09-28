namespace HiveSpace.UserService.Application.Models.Responses.Admin;

public record CreateAdminResponseDto(
    Guid Id,
    string Email,
    string FullName,
    bool IsSystemAdmin,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    DateTimeOffset LastLoginAt,
    bool IsActive = true
);
