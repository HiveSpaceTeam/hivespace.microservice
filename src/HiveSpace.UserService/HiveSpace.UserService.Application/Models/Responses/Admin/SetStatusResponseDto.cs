namespace HiveSpace.UserService.Application.Models.Responses.Admin;

/// <summary>
/// Base response DTO for user status update operations
/// Contains common properties shared between user and admin status responses
/// </summary>
public record SetStatusResponseDto(
    Guid Id,
    string Username,
    string FullName,
    string Email,
    int Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt,
    DateTimeOffset? LastLoginAt,
    string? AvatarUrl
);