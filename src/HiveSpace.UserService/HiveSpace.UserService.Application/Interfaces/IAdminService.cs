using HiveSpace.UserService.Application.Models.Requests;
using HiveSpace.UserService.Application.Models.Responses;

namespace HiveSpace.UserService.Application.Services;

public interface IAdminService
{
    Task<CreateAdminResponseDto> CreateAdminAsync(CreateAdminRequestDto request, CancellationToken cancellationToken = default);
}
