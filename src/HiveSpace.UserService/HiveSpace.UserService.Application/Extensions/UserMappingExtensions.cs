using HiveSpace.UserService.Domain.Aggregates.User;
using HiveSpace.UserService.Application.Models.Responses.Admin;

namespace HiveSpace.UserService.Application.Extensions;

/// <summary>
/// Extension methods for mapping Domain User to Application DTOs
/// </summary>
public static class UserMappingExtensions
{
    /// <summary>
    /// Convert Domain User to UserDto for API responses
    /// </summary>
    public static UserDto ToUserDto(this User domainUser)
    {
        return new UserDto(
            domainUser.Id,
            domainUser.UserName,
            domainUser.FullName,
            domainUser.Email.Value,
            (int)domainUser.Status,
            domainUser.IsSeller,
            domainUser.CreatedAt,
            domainUser.UpdatedAt,
            domainUser.LastLoginAt,
            null // AvatarUrl not available in domain model
        );
    }

    /// <summary>
    /// Convert Domain User to AdminDto for API responses
    /// </summary>
    public static AdminDto ToAdminDto(this User domainUser)
    {
        return new AdminDto(
            domainUser.Id,
            domainUser.UserName,
            domainUser.FullName,
            domainUser.Email.Value,
            (int)domainUser.Status,
            domainUser.IsSystemAdmin,
            domainUser.CreatedAt,
            domainUser.UpdatedAt,
            domainUser.LastLoginAt,
            null // AvatarUrl not available in domain model
        );
    }
}