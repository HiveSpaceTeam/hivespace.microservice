using System;
using HiveSpace.UserService.Domain.Aggregates.User;
using HiveSpace.UserService.Domain.Enums;
using HiveSpace.UserService.Domain.Services;
using HiveSpace.UserService.Infrastructure.Identity;
using System.Reflection;
using System.Linq;
using HiveSpace.Core.Exceptions;
using HiveSpace.Core.Exceptions.Models;

namespace HiveSpace.UserService.Infrastructure.Mappers;

/// <summary>
/// Mapper extensions for converting between Domain User and ApplicationUser
/// </summary>
public static class UserMapper
{
    /// <summary>
    /// Convert Domain User to ApplicationUser for Identity operations
    /// </summary>
    public static ApplicationUser ToApplicationUser(this User domainUser)
    {
        return new ApplicationUser
        {
            Id = domainUser.Id,
            UserName = domainUser.UserName,
            Email = domainUser.Email.Value,
            PhoneNumber = domainUser.PhoneNumber?.Value,
            FullName = domainUser.FullName,
            StoreId = domainUser.StoreId,
            DateOfBirth = domainUser.DateOfBirth?.Value.DateTime,
            Gender = domainUser.Gender?.ToString(),
            UserStatus = domainUser.Status.ToString(),
            CreatedAt = domainUser.CreatedAt,
            UpdatedAt = domainUser.UpdatedAt,
            LastLoginAt = domainUser.LastLoginAt,
            // Addresses collection will be mapped separately via EF Core navigation
            Addresses = domainUser.Addresses.ToList()
        };
    }

    /// <summary>
    /// Convert ApplicationUser to Domain User using UserManager for reconstruction
    /// </summary>
    public static User ToDomainUser(this ApplicationUser applicationUser, IEnumerable<string> roleNames)
    {
        if (roleNames == null)
        {
            throw new HiveSpace.Core.Exceptions.ApplicationException(
                [new Error(CommonErrorCode.ArgumentNull, nameof(roleNames))], 
                400, 
                false);
        }
        // Get the single role (enforces one role only business rule)
        var userRole = RoleMapper.GetSingleRole(roleNames)
            ?? throw new HiveSpace.Core.Exceptions.ApplicationException(
                [new Error(CommonErrorCode.InvalidArgument, nameof(roleNames))], 
                400, 
                false);

        // Parse domain value objects using factory methods
        var email = Email.Create(applicationUser.Email ?? string.Empty);
        var phoneNumber = PhoneNumber.CreateOrDefault(applicationUser.PhoneNumber);
        var dateOfBirth = applicationUser.DateOfBirth.HasValue 
            ? new DateOfBirth(new DateTimeOffset(applicationUser.DateOfBirth.Value, TimeSpan.Zero))
            : null;
        var gender = !string.IsNullOrEmpty(applicationUser.Gender) 
            && Enum.TryParse<Gender>(applicationUser.Gender, out var parsedGender) 
            ? parsedGender 
            : (Gender?)null;
        
        // Parse status
        var status = Enum.TryParse<UserStatus>(applicationUser.UserStatus, out var parsedStatus) 
            ? parsedStatus 
            : UserStatus.Active;

        // Use internal rehydration API (visible only to Infrastructure via InternalsVisibleTo)
        return User.Rehydrate(
            id: applicationUser.Id,
            email: email,
            userName: applicationUser.UserName ?? string.Empty,
            passwordHash: applicationUser.PasswordHash ?? string.Empty,
            fullName: applicationUser.FullName ?? string.Empty,
            role: userRole,
            phoneNumber: phoneNumber,
            dateOfBirth: dateOfBirth,
            gender: gender,
            storeId: applicationUser.StoreId,
            status: status,
            createdAt: applicationUser.CreatedAt,
            updatedAt: applicationUser.UpdatedAt,
            lastLoginAt: applicationUser.LastLoginAt,
            addresses: applicationUser.Addresses);
    }
}
