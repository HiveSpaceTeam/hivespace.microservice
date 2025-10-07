namespace HiveSpace.UserService.Application.Models.Responses.Admin;

/// <summary>
/// Response DTO for admin status update operations
/// Extends the base response with admin-specific properties
/// </summary>
public record SetAdminStatusResponseDto(
    Guid Id,
    string Username,
    string FullName,
    string Email,
    int Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt,
    DateTimeOffset? LastLoginAt,
    string? AvatarUrl,
    // Admin-specific properties
    bool IsSystemAdmin
) : SetStatusResponseDto(Id, Username, FullName, Email, Status, CreatedAt, UpdatedAt, LastLoginAt, AvatarUrl);