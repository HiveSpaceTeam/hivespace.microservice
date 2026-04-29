using HiveSpace.Core.Exceptions.Models;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.UserService.Domain.Aggregates.User;
using HiveSpace.UserService.Domain.Exceptions;
using HiveSpace.UserService.Domain.Repositories;
using HiveSpace.UserService.Infrastructure.Data;
using HiveSpace.UserService.Infrastructure.Identity;
using HiveSpace.UserService.Infrastructure.Mappers;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HiveSpace.UserService.Infrastructure.Repositories;

public class SqlUserRepository(
    UserDbContext context,
    UserManager<ApplicationUser> userManager) : IUserRepository
{

    public async Task<User?> GetByIdAsync(Guid id, bool includeDetail = false, CancellationToken cancellationToken = default)
    {
        IQueryable<ApplicationUser> query = context.Users.AsNoTracking();
        if (includeDetail)
            query = query.Include(u => u.Addresses);

        var appUser = await query.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
        if (appUser == null)
            return null;

        return appUser.ToDomainUser();
    }

    public async Task<User?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default)
    {
        var appUser = await context.Users
            .Include(u => u.Addresses)
            .FirstOrDefaultAsync(u => u.Email == email.Value, cancellationToken);

        if (appUser == null)
            return null;

        return appUser.ToDomainUser();
    }

    public async Task<User?> GetByUserNameAsync(string userName, CancellationToken cancellationToken = default)
    {
        var normalizedUserName = userName.ToUpperInvariant();
        var appUser = await context.Users
            .Include(u => u.Addresses)
            .FirstOrDefaultAsync(u => u.NormalizedUserName == normalizedUserName, cancellationToken);

        if (appUser == null)
            return null;

        return appUser.ToDomainUser();
    }

    // Creates a new user with the Identity store and maps back to the domain aggregate
    public async Task<User> CreateUserAsync(User domainUser, string password, CancellationToken cancellationToken = default)
    {
        var appUser = domainUser.ToApplicationUser();

        var result = await userManager.CreateAsync(appUser, password);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            var error = new Error(UserDomainErrorCode.UserCreationFailed, nameof(User));
            throw new Core.Exceptions.ApplicationException([error], 500, false);
        }

        // Role is already set in ToApplicationUser() mapping, no need for AddToRoleAsync
        // Refresh from database to get the created user with its ID
        await context.SaveChangesAsync(cancellationToken);

        return appUser.ToDomainUser();
    }

    public async Task<List<User>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var users = await context.Users
            .Include(u => u.Addresses)
            .ToListAsync(cancellationToken);

        var result = new List<User>(users.Count);
        foreach (var appUser in users)
        {
            result.Add(appUser.ToDomainUser());
        }
        return result;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => await context.SaveChangesAsync(cancellationToken);

    public async Task<User> UpdateUserAsync(User domainUser, CancellationToken cancellationToken = default)
    {
        var appUser = await context.Users
            .Include(u => u.Addresses)
            .FirstOrDefaultAsync(u => u.Id == domainUser.Id, cancellationToken)
            ?? throw new NotFoundException(UserDomainErrorCode.UserNotFound, nameof(User));

        // Update the tracked entity with values from the domain object
        appUser.UpdateApplicationUser(domainUser);

        await context.SaveChangesAsync(cancellationToken);

        return domainUser;
    }

    public async Task<User> UpdateUserAddressesAsync(User domainUser, CancellationToken cancellationToken = default)
    {
        var appUser = await context.Users
            .Include(u => u.Addresses)
            .FirstOrDefaultAsync(u => u.Id == domainUser.Id, cancellationToken)
            ?? throw new NotFoundException(UserDomainErrorCode.UserNotFound, nameof(User));

        // Update ONLY the addresses from the domain object
        appUser.UpdateApplicationUserAddresses(domainUser);

        await context.SaveChangesAsync(cancellationToken);

        return domainUser;
    }

    public async Task<User> RemoveUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var appUser = await context.Users
            .Include(u => u.Addresses)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken)
            ?? throw new NotFoundException(UserDomainErrorCode.UserNotFound, nameof(User));

        if (appUser.IsDeleted)
            throw new ConflictException(UserDomainErrorCode.UserAlreadyDeleted, nameof(User));

        context.Users.Remove(appUser);
        await context.SaveChangesAsync(cancellationToken);

        return appUser.ToDomainUser();
    }
}
