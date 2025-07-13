using HiveSpace.Domain.Shared;
using HiveSpace.IdentityService.Domain.Aggregates;

namespace HiveSpace.IdentityService.Domain.Repositories;

public interface IUserRepository : IRepository<ApplicationUser>
{
    Task<ApplicationUser?> GetByEmailAsync(string email);
    Task<ApplicationUser?> GetByUserNameAsync(string userName);
    Task<bool> IsEmailExistsAsync(string email);
    Task<bool> IsUserNameExistsAsync(string userName);
}