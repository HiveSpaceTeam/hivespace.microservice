using HiveSpace.Domain.Shared.Interfaces;
using HiveSpace.UserService.Domain.Aggregates.User;
using HiveSpace.UserService.Domain.Enums;

namespace HiveSpace.UserService.Domain.Repositories;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByUserNameAsync(string userName);
    Task<IEnumerable<User>> GetByStatusAsync(UserStatus status);
    Task<IEnumerable<User>> GetCustomersAsync();
    Task<IEnumerable<User>> GetSellersAsync();
    Task<bool> EmailExistsAsync(string email);
    Task<bool> UserNameExistsAsync(string userName);
    Task<IEnumerable<User>> GetPaginatedAsync(int page, int pageSize);
    Task<int> GetTotalCountAsync();
}