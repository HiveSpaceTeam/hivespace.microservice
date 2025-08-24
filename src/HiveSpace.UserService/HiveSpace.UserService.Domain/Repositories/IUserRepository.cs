using HiveSpace.Domain.Shared.Interfaces;
using HiveSpace.UserService.Domain.Aggregates.User;

namespace HiveSpace.UserService.Domain.Repositories;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default);
    Task<User?> GetByUserNameAsync(string userName, CancellationToken cancellationToken = default);
}