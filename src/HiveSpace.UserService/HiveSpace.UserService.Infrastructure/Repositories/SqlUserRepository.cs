using HiveSpace.Core.Exceptions.Models;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.UserService.Domain.Aggregates.User;
using HiveSpace.UserService.Domain.Exceptions;
using HiveSpace.UserService.Domain.Repositories;
using HiveSpace.UserService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HiveSpace.UserService.Infrastructure.Repositories;

public class SqlUserRepository(UserDbContext context) : IUserRepository
{

    public async Task<User?> GetByIdAsync(Guid id, bool includeDetail = false, CancellationToken cancellationToken = default, bool asTracking = false)
    {
        IQueryable<User> query = asTracking
            ? context.Users
            : context.Users.AsNoTracking();

        if (includeDetail)
            query = query.Include(u => u.Addresses);

        return await query.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public async Task<User?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default)
    {
        return await context.Users
            .Include(u => u.Addresses)
            .FirstOrDefaultAsync(u => u.Email == email.Value, cancellationToken);
    }

    public async Task<User?> GetByUserNameAsync(string userName, CancellationToken cancellationToken = default)
    {
        var normalizedUserName = userName.ToUpperInvariant();
        return await context.Users
            .Include(u => u.Addresses)
            .FirstOrDefaultAsync(u => u.UserName.ToUpper() == normalizedUserName, cancellationToken);
    }

    // Creates a profile aggregate only. Credentials and roles are owned by IdentityService.
    public async Task<User> CreateUserAsync(User domainUser, string password, CancellationToken cancellationToken = default)
    {
        context.Users.Add(domainUser);
        await context.SaveChangesAsync(cancellationToken);

        return domainUser;
    }

    public async Task<List<User>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await context.Users
            .AsNoTracking()
            .Include(u => u.Addresses)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => await context.SaveChangesAsync(cancellationToken);

    public async Task<User> RemoveUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await context.Users
            .Include(u => u.Addresses)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken)
            ?? throw new NotFoundException(UserDomainErrorCode.UserNotFound, nameof(User));

        if (user.IsDeleted)
            throw new ConflictException(UserDomainErrorCode.UserAlreadyDeleted, nameof(User));

        context.Users.Remove(user);
        await context.SaveChangesAsync(cancellationToken);

        return user;
    }
}
