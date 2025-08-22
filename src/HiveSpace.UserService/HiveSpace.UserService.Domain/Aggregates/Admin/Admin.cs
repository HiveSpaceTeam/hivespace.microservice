using HiveSpace.Domain.Shared.Entities;
using HiveSpace.Domain.Shared.Interfaces;
using HiveSpace.UserService.Domain.Aggregates.User;
using HiveSpace.UserService.Domain.Enums;
using HiveSpace.UserService.Domain.Exceptions;

namespace HiveSpace.UserService.Domain.Aggregates.Admin;

public class Admin : AggregateRoot<Guid>, IAuditable
{
    // Identity
    public Email Email { get; private set; }
    public string PasswordHash { get; private set; }
    public AdminStatus Status { get; private set; }
    public bool IsSystem { get; private set; }
    
    // Audit
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }
    public DateTimeOffset? LastLoginAt { get; private set; }
    
    private Admin() 
    {
    }
    
    // Domain Methods - Internal to force creation through AdminManager
    internal static Admin Create(Email email, string passwordHash, bool isSystem = false)
    {
        return new Admin
        {
            Email = email,
            PasswordHash = passwordHash,
            Status = AdminStatus.Active,
            IsSystem = isSystem,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }
    
    public void ChangePassword(string newPasswordHash)
    {
        if (IsSystem)
            throw new CannotModifySystemAdminException();
            
        PasswordHash = newPasswordHash;
        UpdatedAt = DateTimeOffset.UtcNow;

    }
    
    public void Activate()
    {
        Status = AdminStatus.Active;
        UpdatedAt = DateTimeOffset.UtcNow;

    }
    
    public void Deactivate()
    {
        if (IsSystem)
            throw new CannotModifySystemAdminException();
            
        Status = AdminStatus.Inactive;
        UpdatedAt = DateTimeOffset.UtcNow;

    }
    
    public void UpdateLastLogin()
    {
        LastLoginAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
    
    public bool IsActive => Status == AdminStatus.Active;
    public bool IsSystemAdmin => IsSystem;
}