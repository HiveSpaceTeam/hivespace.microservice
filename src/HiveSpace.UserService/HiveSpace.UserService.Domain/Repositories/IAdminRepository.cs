using HiveSpace.Domain.Shared.Interfaces;
using HiveSpace.UserService.Domain.Aggregates.Admin;
using HiveSpace.UserService.Domain.Aggregates.User;
using HiveSpace.UserService.Domain.Enums;

namespace HiveSpace.UserService.Domain.Repositories;

public interface IAdminRepository : IRepository<Admin>
{
    Task<Admin?> GetByEmailAsync(Email email);
    Task<IEnumerable<Admin>> GetByStatusAsync(AdminStatus status);
    Task<bool> EmailExistsAsync(Email email);
    Task<IEnumerable<Admin>> GetPaginatedAsync(int page, int pageSize);
    Task<int> GetTotalCountAsync();
}
