using HiveSpace.UserService.Application.Models.Requests.User;
using HiveSpace.UserService.Application.Models.Responses.User;

namespace HiveSpace.UserService.Application.Interfaces.Services;

public interface IUserService
{
    Task<GetUserSettingsResponseDto> GetUserSettingAsync(CancellationToken cancellationToken = default);
    Task SetUserSettingAsync(UpdateUserSettingRequestDto request, CancellationToken cancellationToken = default);
}