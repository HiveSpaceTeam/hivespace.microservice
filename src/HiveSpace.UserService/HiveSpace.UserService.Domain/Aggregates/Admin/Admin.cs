using HiveSpace.Domain.Shared.Entities;
using HiveSpace.Domain.Shared.Exceptions;
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
        ValidateAndThrow(email, passwordHash);
        
        return new Admin
        {
            Email = email,
            PasswordHash = passwordHash,
            Status = AdminStatus.Active,
            IsSystem = isSystem,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }
    
    private static void ValidateAndThrow(Email? email, string? passwordHash)
    {
        if (email == null)
            throw new InvalidEmailException();
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new InvalidPasswordHashException();
    }
    
    public void ChangePassword(string newPasswordHash)
    {
        if (IsSystem)
            throw new ForbiddenException(UserDomainErrorCode.CannotModifySystemAdmin, nameof(Admin));
        
        if (string.IsNullOrWhiteSpace(newPasswordHash))
            throw new InvalidPasswordHashException();
            
        PasswordHash = newPasswordHash;
    }
    
    public void Activate()
    {
        Status = AdminStatus.Active;
    }
    
    public void Deactivate()
    {
        if (IsSystem)
            throw new ForbiddenException(UserDomainErrorCode.CannotModifySystemAdmin, nameof(Admin));
            
        Status = AdminStatus.Inactive;

    }
    
    public void UpdateLastLogin()
    {
        LastLoginAt = DateTimeOffset.UtcNow;
    }
    
    public bool IsActive => Status == AdminStatus.Active;
    public bool IsSystemAdmin => IsSystem;
}