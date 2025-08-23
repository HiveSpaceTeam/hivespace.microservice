using HiveSpace.Domain.Shared.Interfaces;
using HiveSpace.UserService.Domain.Aggregates.Admin;
using HiveSpace.UserService.Domain.Aggregates.User;
using HiveSpace.UserService.Domain.Enums;

namespace HiveSpace.UserService.Domain.Repositories;

public interface IAdminRepository : IRepository<Admin>
{
    Task<Admin?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default);
    Task<IEnumerable<Admin>> GetByStatusAsync(AdminStatus status, CancellationToken cancellationToken = default);
    Task<bool> EmailExistsAsync(Email email, CancellationToken cancellationToken = default);
    Task<IEnumerable<Admin>> GetPaginatedAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> GetTotalCountAsync(CancellationToken cancellationToken = default);
}
