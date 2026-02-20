using HiveSpace.UserService.Domain.Aggregates.User;

namespace HiveSpace.UserService.Domain.Repositories;

public interface IUserRepository
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task<List<User>> GetAllAsync();
    Task<User?> GetByIdAsync(Guid id, bool includeDetail = false);
    Task<User?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default);
    Task<User?> GetByUserNameAsync(string userName, CancellationToken cancellationToken = default);
    Task<User> CreateUserAsync(User domainUser, string password, CancellationToken cancellationToken = default);
    Task<User> UpdateUserAsync(User domainUser, CancellationToken cancellationToken = default);
    Task<User> RemoveUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<User> UpdateUserAddressesAsync(User domainUser, CancellationToken cancellationToken = default);
}