namespace HiveSpace.Core.Contexts;
public interface IUserContext
{
    bool IsAuthenticated { get; }
    Guid UserId { get; }
    string PhoneNumber { get; }
    string Email { get; }
    IReadOnlyList<string> Roles { get; }
    bool IsAdmin { get; }
    bool IsSystemAdmin { get; }
    bool IsSeller { get; }
    Guid? StoreId { get; }
}
