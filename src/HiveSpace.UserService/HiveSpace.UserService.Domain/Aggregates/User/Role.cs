using HiveSpace.Domain.Shared.Entities;

namespace HiveSpace.UserService.Domain.Aggregates.User;

/// <summary>
/// Role value object representing user roles in the system.
/// Each user has exactly one role.
/// </summary>
public class Role : ValueObject
{

    public static class RoleNames
    {
        public const string Seller = "Seller";
        public const string Admin = "Admin";
        public const string SystemAdmin = "SystemAdmin";
    }

    public string Name { get; }
    public string Description { get; }

    private Role(string name, string description)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Role name cannot be empty.", nameof(name));

        Name = name;
        Description = description;
    }


    // Cache Role instances so they’re only created once
    private static readonly Role _seller = new(RoleNames.Seller, "Seller role with product management capabilities.");
    private static readonly Role _admin = new(RoleNames.Admin, "Administrator role with system access.");
    private static readonly Role _systemAdmin = new(RoleNames.SystemAdmin, "System Administrator role with full system access.");

    public static Role Seller => _seller;
    public static Role Admin => _admin;
    public static Role SystemAdmin => _systemAdmin;

    public static Role FromName(string name)
    {
        switch (name)
        {
            case var n when n.Equals(RoleNames.Seller, StringComparison.OrdinalIgnoreCase):
                return Seller;
            case var n when n.Equals(RoleNames.Admin, StringComparison.OrdinalIgnoreCase):
                return Admin;
            case var n when n.Equals(RoleNames.SystemAdmin, StringComparison.OrdinalIgnoreCase):
                return SystemAdmin;
            default:
                throw new ArgumentException($"Unknown role name: {name}");
        }
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Name;
    }

    public static implicit operator string(Role role) => role.Name;
    public static explicit operator Role(string name) => FromName(name);

    // Factory methods for better creation control
    public static Role Create(string name) => FromName(name);
    public static Role? CreateOrDefault(string? name) => 
        string.IsNullOrWhiteSpace(name) ? null : FromName(name);

    public override string ToString() => Name;
}
