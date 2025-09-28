using HiveSpace.Core.Models.Pagination;
using HiveSpace.UserService.Application.Models.Queries;
using HiveSpace.UserService.Application.Models.Responses.Admin;

namespace HiveSpace.UserService.Application.Interfaces.DataQueries;

public interface IAdminDataQuery
{
    Task<PagedResult<AdminDto>> GetPagingAdminsAsync(AdminUserFilterRequest request, CancellationToken cancellationToken = default);
    Task<int> GetTotalAdminsCountAsync(AdminUserFilterRequest request, CancellationToken cancellationToken = default);
}
