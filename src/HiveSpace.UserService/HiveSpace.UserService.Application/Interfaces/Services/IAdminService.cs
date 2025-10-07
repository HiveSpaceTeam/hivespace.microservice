using HiveSpace.UserService.Application.Constant.Enum;
using HiveSpace.UserService.Application.Models.Requests.Admin;
using HiveSpace.UserService.Application.Models.Responses.Admin;

namespace HiveSpace.UserService.Application.Interfaces.Services;

public interface IAdminService
{
    Task<CreateAdminResponseDto> CreateAdminAsync(CreateAdminRequestDto request, CancellationToken cancellationToken = default);
    Task<GetUsersResponseDto> GetUsersAsync(GetUsersRequestDto request, CancellationToken cancellationToken = default);
    Task<GetAdminResponseDto> GetAdminsAsync(GetAdminRequestDto request, CancellationToken cancellationToken = default);
    
    // Method that returns strongly-typed user/admin DTO based on ResponseType
    Task<SetStatusResponseDto> SetUserStatusAsync(SetUserStatusRequestDto request, CancellationToken cancellationToken = default);
    
    // New unified method
    Task<GetUnifiedUsersResponseDto<T>> GetUnifiedUsersAsync<T>(GetUsersBaseRequestDto request, UserQueryType queryType, CancellationToken cancellationToken = default) where T : class;
}
