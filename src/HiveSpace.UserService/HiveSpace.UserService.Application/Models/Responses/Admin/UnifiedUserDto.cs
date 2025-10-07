namespace HiveSpace.UserService.Application.Models.Responses.Admin;

public record UnifiedUserDto(
    Guid Id,
    string Username,
    string FullName,
    string Email,
    int Status,
    bool IsSeller,
    bool IsSystemAdmin,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt,
    DateTimeOffset? LastLoginAt,
    string? AvatarUrl
)
{
    // Convenience method to convert to UserDto
    public UserDto ToUserDto() => new(
        Id, Username, FullName, Email, Status, IsSeller, 
        CreatedAt, UpdatedAt, LastLoginAt, AvatarUrl);

    // Convenience method to convert to AdminDto
    public AdminDto ToAdminDto() => new(
        Id, Username, FullName, Email, Status, IsSystemAdmin, 
        CreatedAt, UpdatedAt, LastLoginAt, AvatarUrl);
};