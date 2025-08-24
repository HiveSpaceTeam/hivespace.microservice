using HiveSpace.Infrastructure.Persistence.Repositories;
using HiveSpace.UserService.Domain.Aggregates.Admin;
using HiveSpace.UserService.Domain.Aggregates.User;
using HiveSpace.UserService.Domain.Repositories;
using HiveSpace.UserService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HiveSpace.UserService.Infrastructure.Repositories;

public class AdminRepository : BaseRepository<Admin, Guid>, IAdminRepository
{
    public AdminRepository(UserDbContext context) : base(context)
    {
    }

    public async Task<Admin?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default)
    {
        return await _context.Set<Admin>()
            .FirstOrDefaultAsync(a => a.Email.Value == email.Value, cancellationToken);
    }
}