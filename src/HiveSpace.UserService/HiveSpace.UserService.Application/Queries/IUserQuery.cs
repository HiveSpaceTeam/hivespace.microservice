using HiveSpace.Core.Models.Pagination;
using HiveSpace.UserService.Application.Models.Responses.Admin;
using HiveSpace.UserService.Domain.Models;

namespace HiveSpace.UserService.Application.Queries;

public interface IUserQuery
{
    Task<PagedResult<UserListItemDto>> GetPagingUsersAsync(AdminUserFilterRequest request, CancellationToken cancellationToken = default);
    Task<int> GetTotalUsersCountAsync(AdminUserFilterRequest request, CancellationToken cancellationToken = default);
}