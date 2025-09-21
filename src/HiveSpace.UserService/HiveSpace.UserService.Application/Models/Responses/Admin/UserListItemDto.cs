namespace HiveSpace.UserService.Application.Models.Responses.Admin;

public record UserDto(
    Guid Id,
    string Username,
    string FullName,
    string Email,
    int Status,
    bool IsSeller,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt,
    DateTimeOffset? LastLoginAt,
    string? AvatarUrl
);
