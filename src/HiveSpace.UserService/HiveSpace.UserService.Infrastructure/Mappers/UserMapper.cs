using System;
using HiveSpace.UserService.Domain.Aggregates.User;
using HiveSpace.UserService.Domain.Enums;
using HiveSpace.UserService.Domain.Services;
using HiveSpace.UserService.Infrastructure.Identity;
using System.Reflection;
using System.Linq;
using HiveSpace.Core.Exceptions;
using HiveSpace.Core.Exceptions.Models;
using Microsoft.EntityFrameworkCore;

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
            EmailConfirmed = domainUser.EmailConfirmed,
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
            Addresses = [.. domainUser.Addresses],
            IsDeleted = domainUser.IsDeleted,
            DeletedAt = domainUser.DeletedAt,
            // Map settings as primitive values
            Theme = domainUser.Settings.Theme,
            Culture = domainUser.Settings.Culture
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
        var user = User.Rehydrate(
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
            emailConfirmed: applicationUser.EmailConfirmed,
            createdAt: applicationUser.CreatedAt,
            updatedAt: applicationUser.UpdatedAt,
            lastLoginAt: applicationUser.LastLoginAt,
            addresses: applicationUser.Addresses,
            isDeleted: applicationUser.IsDeleted,
            deletedAt: applicationUser.DeletedAt,
            theme: applicationUser.Theme,
            culture: applicationUser.Culture);
        return user;
    }

    /// <summary>
    /// Update ApplicationUser with changes from Domain User
    /// Preserves EF Core change tracking
    /// </summary>
    /// <summary>
    /// Update ApplicationUser with changes from Domain User
    /// Preserves EF Core change tracking and updates related collections
    /// </summary>
    public static void UpdateApplicationUser(this ApplicationUser applicationUser, User domainUser)
    {
        applicationUser.UserName = domainUser.UserName;
        applicationUser.Email = domainUser.Email.Value;
        applicationUser.PhoneNumber = domainUser.PhoneNumber?.Value;
        applicationUser.FullName = domainUser.FullName;
        applicationUser.StoreId = domainUser.StoreId;
        applicationUser.DateOfBirth = domainUser.DateOfBirth?.Value.DateTime;
        applicationUser.Gender = (int?)domainUser.Gender;
        applicationUser.Status = (int)domainUser.Status;
        applicationUser.UpdatedAt = DateTimeOffset.UtcNow;
        applicationUser.LastLoginAt = domainUser.LastLoginAt;
        applicationUser.RoleName = domainUser.Role?.Name; // Update role name directly
        applicationUser.Theme = domainUser.Settings.Theme;
        applicationUser.Culture = domainUser.Settings.Culture;

        // Synchronize addresses collection
        applicationUser.UpdateApplicationUserAddresses(domainUser);
    }

    /// <summary>
    /// Update only the addresses collection of ApplicationUser from Domain User
    /// </summary>
    public static void UpdateApplicationUserAddresses(this ApplicationUser applicationUser, User domainUser)
    {
        // 1. Remove addresses that are no longer in the domain user
        var addressesToRemove = applicationUser.Addresses
            .Where(a => !domainUser.Addresses.Any(da => da.Id == a.Id))
            .ToList();

        foreach (var address in addressesToRemove)
        {
            applicationUser.Addresses.Remove(address);
        }

        // 2. Add or Update addresses
        foreach (var domainAddress in domainUser.Addresses)
        {
            var existingAddress = applicationUser.Addresses
                .FirstOrDefault(a => a.Id == domainAddress.Id);

            if (existingAddress is null)
            {
                applicationUser.Addresses.Add(domainAddress);
            }
            else
            {
                existingAddress.UpdateDetails(
                    domainAddress.FullName,
                    domainAddress.PhoneNumber,
                    domainAddress.Street,
                    domainAddress.District,
                    domainAddress.Province,
                    domainAddress.Country,
                    domainAddress.ZipCode,
                    domainAddress.AddressType
                );
                
                if (domainAddress.IsDefault) existingAddress.SetAsDefault();
                else existingAddress.RemoveDefaultStatus();
            }
        }
    }
}
