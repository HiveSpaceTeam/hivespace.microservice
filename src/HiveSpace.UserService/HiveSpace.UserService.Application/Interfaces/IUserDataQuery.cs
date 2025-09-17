using HiveSpace.Core.Models.Pagination;
using HiveSpace.UserService.Application.Models.Queries;
using HiveSpace.UserService.Application.Models.Responses.Admin;

namespace HiveSpace.UserService.Application.Interfaces;

public interface IUserDataQuery
{
    Task<PagedResult<UserListItemDto>> GetPagingUsersAsync(AdminUserFilterRequest request, CancellationToken cancellationToken = default);
    Task<int> GetTotalUsersCountAsync(AdminUserFilterRequest request, CancellationToken cancellationToken = default);
}