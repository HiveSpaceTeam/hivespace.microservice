using HiveSpace.Domain.Shared.Interfaces;
using HiveSpace.UserService.Domain.Aggregates.User;
using HiveSpace.UserService.Domain.Enums;

namespace HiveSpace.UserService.Domain.Repositories;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default);
    Task<User?> GetByUserNameAsync(string userName, CancellationToken cancellationToken = default);
    Task<IEnumerable<User>> GetByStatusAsync(UserStatus status, CancellationToken cancellationToken = default);
    Task<IEnumerable<User>> GetCustomersAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<User>> GetSellersAsync(CancellationToken cancellationToken = default);
    Task<bool> EmailExistsAsync(Email email, CancellationToken cancellationToken = default);
    Task<bool> UserNameExistsAsync(string userName, CancellationToken cancellationToken = default);
    Task<IEnumerable<User>> GetPaginatedAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> GetTotalCountAsync(CancellationToken cancellationToken = default);
}