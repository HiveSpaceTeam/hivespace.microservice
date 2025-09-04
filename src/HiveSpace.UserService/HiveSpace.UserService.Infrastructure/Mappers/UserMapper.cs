using System;
using HiveSpace.UserService.Domain.Aggregates.User;
using HiveSpace.UserService.Domain.Enums;
using HiveSpace.UserService.Domain.Services;
using HiveSpace.UserService.Infrastructure.Identity;
using HiveSpace.UserService.Infrastructure.Mappers;

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
    public static User ToDomainUser(this ApplicationUser applicationUser, IEnumerable<string> roleNames, UserManager userManager)
    {
        if (roleNames == null)
        {
            throw new ArgumentNullException(nameof(roleNames), "Role names cannot be null.");
        }
        // Get the single role (enforces one role only business rule)
        var userRole = RoleMapper.GetSingleRole(roleNames);
        if (userRole == null)
        {
            throw new ArgumentException("User must have a valid role assigned", nameof(roleNames));
        }
        
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

        // Use UserManager to reconstruct the domain user
        return userManager.ReconstructUser(
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
            lastLoginAt: applicationUser.LastLoginAt);
    }

    /// <summary>
    /// Update ApplicationUser with changes from Domain User
    /// Preserves EF Core change tracking
    /// </summary>
    public static void UpdateApplicationUser(this ApplicationUser applicationUser, User domainUser)
    {
        applicationUser.UserName = domainUser.UserName;
        applicationUser.Email = domainUser.Email.Value;
        applicationUser.PhoneNumber = domainUser.PhoneNumber?.Value;
        applicationUser.FullName = domainUser.FullName;
        applicationUser.UpdatedAt = DateTimeOffset.UtcNow;
        applicationUser.DateOfBirth = domainUser.DateOfBirth?.Value.DateTime;
        applicationUser.Gender = domainUser.Gender?.ToString();
        applicationUser.UserStatus = domainUser.Status.ToString();
        applicationUser.UpdatedAt = DateTime.UtcNow;
        // Note: Addresses are updated separately via EF Core change tracking
    }
}
