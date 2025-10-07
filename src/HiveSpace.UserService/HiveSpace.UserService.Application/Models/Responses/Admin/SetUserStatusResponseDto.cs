namespace HiveSpace.UserService.Application.Models.Responses.Admin;

/// <summary>
/// Response DTO for user status update operations
/// Extends the base response with user-specific properties
/// </summary>
public record SetUserStatusResponseDto(
    Guid Id,
    string Username,
    string FullName,
    string Email,
    int Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt,
    DateTimeOffset? LastLoginAt,
    string? AvatarUrl,
    // User-specific properties
    bool IsSeller
) : SetStatusResponseDto(Id, Username, FullName, Email, Status, CreatedAt, UpdatedAt, LastLoginAt, AvatarUrl);