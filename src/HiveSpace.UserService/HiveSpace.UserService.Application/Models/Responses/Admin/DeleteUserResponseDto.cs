namespace HiveSpace.UserService.Application.Models.Responses.Admin;

/// <summary>
/// Response DTO for user deletion operations
/// Contains user information after soft deletion
/// </summary>
public record DeleteUserResponseDto(
    Guid Id,
    string Username,
    string FullName,
    string Email,
    int Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt,
    DateTimeOffset? LastLoginAt,
    DateTimeOffset? DeletedAt,
    string? AvatarUrl,
    // User-specific properties
    bool IsSeller,
    string DeletedBy
);
