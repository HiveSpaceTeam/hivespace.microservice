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

    /// <summary>
    /// Convert Domain User to SetUserStatusResponseDto for status update responses
    /// </summary>
    public static SetUserStatusResponseDto ToSetUserStatusResponseDto(this User domainUser)
    {
        return new SetUserStatusResponseDto(
            domainUser.Id,
            domainUser.UserName,
            domainUser.FullName,
            domainUser.Email.Value,
            (int)domainUser.Status,
            domainUser.CreatedAt,
            domainUser.UpdatedAt,
            domainUser.LastLoginAt,
            null, // AvatarUrl not available in domain model
            domainUser.IsSeller
        );
    }

    /// <summary>
    /// Convert Domain User to SetAdminStatusResponseDto for status update responses
    /// </summary>
    public static SetAdminStatusResponseDto ToSetAdminStatusResponseDto(this User domainUser)
    {
        return new SetAdminStatusResponseDto(
            domainUser.Id,
            domainUser.UserName,
            domainUser.FullName,
            domainUser.Email.Value,
            (int)domainUser.Status,
            domainUser.CreatedAt,
            domainUser.UpdatedAt,
            domainUser.LastLoginAt,
            null, // AvatarUrl not available in domain model
            domainUser.IsSystemAdmin
        );
    }

    /// <summary>
    /// Convert Domain User to DeleteUserResponseDto for user deletion responses
    /// </summary>
    public static DeleteUserResponseDto ToDeleteUserResponseDto(this User domainUser, string deletedBy)
    {
        return new DeleteUserResponseDto(
            domainUser.Id,
            domainUser.UserName,
            domainUser.FullName,
            domainUser.Email.Value,
            (int)domainUser.Status,
            domainUser.CreatedAt,
            domainUser.UpdatedAt,
            domainUser.LastLoginAt,
            domainUser.DeletedAt,
            null, // AvatarUrl not available in domain model
            domainUser.IsSeller,
            deletedBy
        );
    }
}