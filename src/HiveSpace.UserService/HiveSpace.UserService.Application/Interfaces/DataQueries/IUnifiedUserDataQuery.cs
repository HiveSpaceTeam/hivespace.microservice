using HiveSpace.Core.Models.Pagination;
using HiveSpace.UserService.Application.Constant.Enum;
using HiveSpace.UserService.Application.DTOs.Admin;

namespace HiveSpace.UserService.Application.Interfaces.DataQueries;

public interface IUnifiedUserDataQuery
{
    Task<PagedResult<UnifiedUserDto>> GetPagingUsersAsync(AdminUserFilterRequest request, UserQueryType queryType, CancellationToken cancellationToken = default);
    Task<int> GetTotalUsersCountAsync(AdminUserFilterRequest request, UserQueryType queryType, CancellationToken cancellationToken = default);
}