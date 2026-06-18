using HiveSpace.Core.Contexts;

namespace HiveSpace.Testing.Shared.Doubles;

public sealed class FakeUserContext : IUserContext
{
    public bool IsAuthenticated => true;
    public required Guid UserId { get; init; }
    public string PhoneNumber => string.Empty;
    public string Email => "test@hivespace.local";
    public IReadOnlyList<string> Roles { get; init; } = ["Buyer"];
    public bool IsAdmin => Roles.Contains("Admin", StringComparer.OrdinalIgnoreCase);
    public bool IsSystemAdmin => Roles.Contains("SystemAdmin", StringComparer.OrdinalIgnoreCase);
    public bool IsSeller => Roles.Contains("Seller", StringComparer.OrdinalIgnoreCase);
    public bool IsBuyer => Roles.Contains("Buyer", StringComparer.OrdinalIgnoreCase);
    public Guid? StoreId { get; init; }
    public string? FullName => null;
    public string? Locale => null;
}
