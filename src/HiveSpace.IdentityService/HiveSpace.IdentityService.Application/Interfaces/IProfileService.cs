using HiveSpace.IdentityService.Application.Models.Requests;
using HiveSpace.IdentityService.Application.Models.Responses;

namespace HiveSpace.IdentityService.Application.Interfaces;

public interface IProfileService
{
    Task<SignupResponseDto> CreateUserAsync(SignupRequestDto requestDto);
    Task UpdateUserInfoAsync(UpdateUserRequestDto param);
    Task ChangePassword(ChangePasswordRequestDto requestDto);
}
