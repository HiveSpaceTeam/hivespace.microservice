namespace HiveSpace.UserService.Application.Models.Responses.Admin;

public record AdminDto(
    Guid Id,
    string Username,
    string FullName,
    string Email,
    int Status,
    bool IsSystemAdmin,
    DateTime CreatedDate,
    DateTime? LastUpdatedDate,
    DateTime? LastLoginDate,
    string? Avatar
);
