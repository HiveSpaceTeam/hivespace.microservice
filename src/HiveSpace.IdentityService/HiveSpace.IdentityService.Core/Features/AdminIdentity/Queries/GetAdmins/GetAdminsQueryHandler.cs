using HiveSpace.Application.Shared.Handlers;
using HiveSpace.Core.Models.Pagination;
using HiveSpace.IdentityService.Core.Features.AdminIdentity.Dtos;
using HiveSpace.IdentityService.Core.Identity;
using HiveSpace.IdentityService.Core.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HiveSpace.IdentityService.Core.Features.AdminIdentity.Queries.GetAdmins;

public class GetAdminsQueryHandler(IdentityDbContext dbContext) : IQueryHandler<GetAdminsQuery, GetAdminsResult>
{
    public async Task<GetAdminsResult> Handle(GetAdminsQuery query, CancellationToken cancellationToken)
    {
        var usersQuery = ApplySearch(dbContext.Users.AsNoTracking()
            .Where(u => u.RoleName == "Admin" || u.RoleName == "SystemAdmin"), query.SearchTerm);

        var total = await usersQuery.CountAsync(cancellationToken);
        var users = await usersQuery
            .OrderByDescending(u => u.CreatedAt)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(u => AdminIdentityMapper.ToAdminDto(u))
            .ToListAsync(cancellationToken);

        return new GetAdminsResult(users, new PaginationMetadata(query.Page, query.PageSize, total));
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
