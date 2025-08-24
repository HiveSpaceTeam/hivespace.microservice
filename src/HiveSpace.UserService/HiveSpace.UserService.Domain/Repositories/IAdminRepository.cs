using HiveSpace.Domain.Shared.Interfaces;
using HiveSpace.UserService.Domain.Aggregates.Admin;
using HiveSpace.UserService.Domain.Aggregates.User;

namespace HiveSpace.UserService.Domain.Repositories;

public interface IAdminRepository : IRepository<Admin>
{
    Task<Admin?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default);
}
