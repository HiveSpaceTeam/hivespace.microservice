using HiveSpace.Infrastructure.Persistence.Repositories;
using HiveSpace.UserService.Domain.Aggregates.User;
using HiveSpace.UserService.Domain.Repositories;
using HiveSpace.UserService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HiveSpace.UserService.Infrastructure.Repositories;

public class UserRepository : BaseRepository<User, Guid>, IUserRepository
{
    public UserRepository(UserDbContext context) : base(context)
    {
    }

    protected override IQueryable<User> ApplyIncludeDetail(IQueryable<User> query)
    {
        return query.Include(u => u.Addresses);
    }

    public async Task<User?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default)
    {
        return await _context.Set<User>()
            .FirstOrDefaultAsync(u => u.Email.Value == email.Value, cancellationToken);
    }

    public async Task<User?> GetByUserNameAsync(string userName, CancellationToken cancellationToken = default)
    {
        return await _context.Set<User>()
            .FirstOrDefaultAsync(u => u.UserName == userName, cancellationToken);
    }
}