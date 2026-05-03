using HiveSpace.UserService.Application.DTOs.User;

namespace HiveSpace.UserService.Application.Interfaces.Services;

public interface IUserService
{
    Task<GetUserSettingsResponseDto> GetUserSettingAsync(CancellationToken cancellationToken = default);
    Task SetUserSettingAsync(UpdateUserSettingRequestDto request, CancellationToken cancellationToken = default);
    Task<GetUserProfileResponseDto> GetUserProfileAsync(CancellationToken cancellationToken = default);
    Task UpdateUserProfileAsync(UpdateUserProfileRequestDto request, CancellationToken cancellationToken = default);
}
