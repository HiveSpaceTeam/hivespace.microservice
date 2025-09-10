using HiveSpace.UserService.Domain.Aggregates.User;
using HiveSpace.UserService.Domain.Repositories;
using HiveSpace.UserService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using HiveSpace.UserService.Infrastructure.Identity;
using HiveSpace.UserService.Infrastructure.Mappers;
using HiveSpace.Core.Exceptions.Models;
using HiveSpace.UserService.Domain.Exceptions;

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

        var roleNames = await _userManager.GetRolesAsync(appUser);
        return appUser.ToDomainUser(roleNames);
    }

    public async Task<User?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default)
    {
        var appUser = await _context.Users
            .Include(u => u.Addresses)
            .FirstOrDefaultAsync(u => u.Email == email.Value, cancellationToken);

        if (appUser == null)
            return null;

        var roleNames = await _userManager.GetRolesAsync(appUser);
        return appUser.ToDomainUser(roleNames);
    }

    public async Task<User?> GetByUserNameAsync(string userName, CancellationToken cancellationToken = default)
    {
        var appUser = await _context.Users
            .Include(u => u.Addresses)
            .FirstOrDefaultAsync(u => u.UserName == userName, cancellationToken);

        if (appUser == null)
            return null;

        var roleNames = await _userManager.GetRolesAsync(appUser);
        return appUser.ToDomainUser(roleNames);
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

        if (domainUser.Role != null)
        {
            var roleName = RoleMapper.ToRoleName(domainUser.Role);
            await _userManager.AddToRoleAsync(appUser, roleName);
        }
        return appUser.ToDomainUser(await _userManager.GetRolesAsync(appUser));
    }

    public async Task<List<User>> GetAllAsync()
    {
        var users = await _context.Users
            .Include(u => u.Addresses)
            .Include(u => u.UserRoles) // Include Store navigation property
            .ToListAsync();

        var result = new List<User>(users.Count);
        foreach (var appUser in users)
        {
            var roles = await _userManager.GetRolesAsync(appUser);
            result.Add(appUser.ToDomainUser(roles));
        }
        return result;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => await _context.SaveChangesAsync(cancellationToken);
}