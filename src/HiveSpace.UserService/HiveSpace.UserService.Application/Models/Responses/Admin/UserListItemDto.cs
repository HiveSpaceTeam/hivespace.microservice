namespace HiveSpace.UserService.Application.Models.Responses.Admin;

public record UserListItemDto(
    Guid Id,
    string Username,
    string FullName,
    string Email,
    int Status,
    bool IsSeller,
    DateTime CreatedDate,
    DateTime? LastLoginDate,
    string? Avatar
);
