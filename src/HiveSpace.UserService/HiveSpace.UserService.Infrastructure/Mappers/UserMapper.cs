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
            Gender = (int?)domainUser.Gender,
            Status = (int)domainUser.Status,
            RoleName = domainUser.Role?.Name, // Store role name directly
            CreatedAt = domainUser.CreatedAt,
            UpdatedAt = domainUser.UpdatedAt,
            LastLoginAt = domainUser.LastLoginAt,
            // Addresses collection will be mapped separately via EF Core navigation
            Addresses = domainUser.Addresses.ToList()
        };
    }

    /// <summary>
    /// Convert ApplicationUser to Domain User using direct RoleName property
    /// </summary>
    public static User ToDomainUser(this ApplicationUser applicationUser)
    {
        // Get the role directly from RoleName property
        var userRole = !string.IsNullOrEmpty(applicationUser.RoleName) 
            ? Role.FromName(applicationUser.RoleName) 
            : null;

        // Parse domain value objects using factory methods
        var email = Email.Create(applicationUser.Email ?? string.Empty);
        var phoneNumber = PhoneNumber.CreateOrDefault(applicationUser.PhoneNumber);
        var dateOfBirth = applicationUser.DateOfBirth.HasValue 
            ? new DateOfBirth(new DateTimeOffset(applicationUser.DateOfBirth.Value, TimeSpan.Zero))
            : null;
        var gender = applicationUser.Gender.HasValue 
            ? (Gender)applicationUser.Gender.Value 
            : (Gender?)null;
        
        // Parse status
        var status = (UserStatus)applicationUser.Status;

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
