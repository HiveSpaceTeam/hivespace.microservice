namespace HiveSpace.Core.UserContext;
public interface IUserContext
{
    bool IsAuthenticated { get; }
    Guid UserId { get; }
    string PhoneNumber { get; }
    string Email { get; }
}
