using HiveSpace.UserService.Domain.Aggregates.User;
using HiveSpace.UserService.Application.DTOs.Admin;

namespace HiveSpace.UserService.Application.Extensions;

public static class UserMappingExtensions
{
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
            domainUser.AvatarUrl
        );
    }

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
            domainUser.AvatarUrl
        );
    }

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
            domainUser.AvatarUrl,
            domainUser.IsSeller
        );
    }

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
            domainUser.AvatarUrl,
            domainUser.IsSystemAdmin
        );
    }

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
            domainUser.AvatarUrl,
            domainUser.IsSeller,
            deletedBy
        );
    }
}
