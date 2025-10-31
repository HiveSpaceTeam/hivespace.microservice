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

public class UserRepository : IUserRepository
{
    private readonly UserDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public UserRepository(
        UserDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<User?> GetByIdAsync(Guid id, bool includeDetail = false)
    {
        IQueryable<ApplicationUser> query = _context.Users.AsNoTracking();
        if (includeDetail)
            query = query.Include(u => u.Addresses);

        var appUser = await query.FirstOrDefaultAsync(u => u.Id == id);
        if (appUser == null)
            return null;

        return appUser.ToDomainUser();
    }

    public async Task<User?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default)
    {
        var appUser = await _context.Users
            .Include(u => u.Addresses)
            .FirstOrDefaultAsync(u => u.Email == email.Value, cancellationToken);

        if (appUser == null)
            return null;

        return appUser.ToDomainUser();
    }

    public async Task<User?> GetByUserNameAsync(string userName, CancellationToken cancellationToken = default)
    {
        var appUser = await _context.Users
            .Include(u => u.Addresses)
            .FirstOrDefaultAsync(u => u.UserName == userName, cancellationToken);

        if (appUser == null)
            return null;

        return appUser.ToDomainUser();
    }

    // Creates a new user with the Identity store and maps back to the domain aggregate
    public async Task<User> CreateUserAsync(User domainUser, string password, CancellationToken cancellationToken = default)
    {
        var appUser = domainUser.ToApplicationUser();

        var result = await _userManager.CreateAsync(appUser, password);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            var error = new Error(UserDomainErrorCode.UserCreationFailed, nameof(User));
            throw new Core.Exceptions.ApplicationException([error], 500, false);
        }

        // Role is already set in ToApplicationUser() mapping, no need for AddToRoleAsync
        // Refresh from database to get the created user with its ID
        await _context.SaveChangesAsync(cancellationToken);

        return appUser.ToDomainUser();
    }

    public async Task<List<User>> GetAllAsync()
    {
        var users = await _context.Users
            .Include(u => u.Addresses)
            .ToListAsync();

        var result = new List<User>(users.Count);
        foreach (var appUser in users)
        {
            result.Add(appUser.ToDomainUser());
        }
        return result;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => await _context.SaveChangesAsync(cancellationToken);

    public async Task<User> UpdateUserAsync(User domainUser, CancellationToken cancellationToken = default)
    {

        var appUser = await _context.Users
            .Include(u => u.Addresses)
            .FirstOrDefaultAsync(u => u.Id == domainUser.Id, cancellationToken)
            ?? throw new NotFoundException(UserDomainErrorCode.UserNotFound, nameof(User));

        var updatedUser = domainUser.ToApplicationUser();

        appUser.UpdateApplicationUser(domainUser);

        var result = await _context.SaveChangesAsync(cancellationToken);
        return domainUser;
    }

    /// <summary>
    /// Removes a user using EF Core Remove() - SoftDeleteInterceptor handles soft delete automatically
    /// </summary>
    /// <param name="userId">ID of the user to remove</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The removed user</returns>
    public async Task<User> RemoveUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var appUser = await _context.Users
            .Include(u => u.Addresses)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken)
            ?? throw new NotFoundException(UserDomainErrorCode.UserNotFound, nameof(User));

        // Check if already deleted
        if (appUser.IsDeleted)
            throw new ConflictException(UserDomainErrorCode.UserAlreadyDeleted, nameof(User));

        // Use EF Core Remove() - SoftDeleteInterceptor will automatically:
        // 1. Set IsDeleted = true
        // 2. Set DeletedAt = DateTime.UtcNow
        // 3. Change state from Deleted to Modified
        _context.Users.Remove(appUser);
        await _context.SaveChangesAsync(cancellationToken);

        return appUser.ToDomainUser();
    }
}