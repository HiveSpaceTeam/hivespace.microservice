using HiveSpace.Application.Shared.Handlers;
using HiveSpace.Core.Models.Pagination;
using HiveSpace.IdentityService.Core.Features.AdminIdentity.Dtos;
using HiveSpace.IdentityService.Core.Identity;
using HiveSpace.IdentityService.Core.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HiveSpace.IdentityService.Core.Features.AdminIdentity.Queries.GetUsers;

public class GetUsersQueryHandler(IdentityDbContext dbContext) : IQueryHandler<GetUsersQuery, GetUsersResult>
{
    public async Task<GetUsersResult> Handle(GetUsersQuery query, CancellationToken cancellationToken)
    {
        var usersQuery = ApplySearch(dbContext.Users.AsNoTracking()
            .Where(u => u.RoleName == null || u.RoleName == "Buyer" || u.RoleName == "Seller"), query.SearchTerm);

        var total = await usersQuery.CountAsync(cancellationToken);
        var users = await usersQuery
            .OrderByDescending(u => u.CreatedAt)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(u => AdminIdentityMapper.ToUserDto(u))
            .ToListAsync(cancellationToken);

        return new GetUsersResult(users, new PaginationMetadata(query.Page, query.PageSize, total));
    }

    private static IQueryable<ApplicationUser> ApplySearch(IQueryable<ApplicationUser> query, string? searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return query;

        var search = searchTerm.Trim();
        return query.Where(u =>
            (u.Email != null && u.Email.Contains(search)) ||
            (u.UserName != null && u.UserName.Contains(search)));
    }
}
