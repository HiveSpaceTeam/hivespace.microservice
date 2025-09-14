using HiveSpace.UserService.Application.Models.Requests.Admin;
using HiveSpace.UserService.Application.Models.Responses.Admin;

namespace HiveSpace.UserService.Application.Interfaces;

public interface IAdminService
{
    Task<CreateAdminResponseDto> CreateAdminAsync(CreateAdminRequestDto request, CancellationToken cancellationToken = default);
    Task<GetUsersResponseDto> GetUsersAsync(GetUsersRequestDto request, CancellationToken cancellationToken = default);
}
