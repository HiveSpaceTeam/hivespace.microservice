namespace HiveSpace.UserService.Application.Models.Responses.Admin;

public record SetUserStatusResponseDto(
    Guid Id,
    string Username,
    string FullName,
    string Email,
    int Status,
    bool IsSeller,
    bool IsAdmin,
    bool IsSystemAdmin,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt,
    DateTimeOffset? LastLoginAt,
    string? AvatarUrl
);


