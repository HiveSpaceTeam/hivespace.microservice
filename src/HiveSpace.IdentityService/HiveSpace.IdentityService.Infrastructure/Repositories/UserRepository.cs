using HiveSpace.IdentityService.Domain.Repositories;
using HiveSpace.IdentityService.Infrastructure.Data;
using HiveSpace.Infrastructure.EntityFrameworkCore.Repositories;
using Microsoft.AspNetCore.Identity;
using HiveSpace.IdentityService.Domain.Aggregates;
using Microsoft.EntityFrameworkCore;

namespace HiveSpace.IdentityService.Infrastructure.Repositories;

public class UserRepository(IdentityDbContext _context, SignInManager<ApplicationUser> _signInManager) : BaseRepository<ApplicationUser, Guid>(_context), IUserRepository
{
    protected readonly SignInManager<ApplicationUser> signInManager = _signInManager;

    protected override IQueryable<ApplicationUser> ApplyIncludeDetail(IQueryable<ApplicationUser> query)
    {
        return query.Include(x => x.Addresses);
    }

    public async Task<ApplicationUser?> GetByEmailAsync(string email)
    {
        return await _context.Set<ApplicationUser>().FirstOrDefaultAsync(x => x.Email == email);
    }

    public async Task<ApplicationUser?> GetByUserNameAsync(string userName)
    {
        return await _context.Set<ApplicationUser>().FirstOrDefaultAsync(x => x.UserName == userName);
    }

    public async Task<bool> IsEmailExistsAsync(string email)
    {
        return await _context.Set<ApplicationUser>().AnyAsync(x => x.Email == email);
    }

    public async Task<bool> IsUserNameExistsAsync(string userName)
    {
        return await _context.Set<ApplicationUser>().AnyAsync(x => x.UserName == userName);
    }
}