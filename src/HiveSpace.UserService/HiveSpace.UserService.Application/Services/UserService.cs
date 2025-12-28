using HiveSpace.Core.Contexts;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.UserService.Application.Interfaces.Messaging;
using HiveSpace.UserService.Application.Interfaces.Services;
using HiveSpace.UserService.Application.Models.Requests.User;
using HiveSpace.UserService.Application.Models.Responses.User;
using HiveSpace.UserService.Domain.Aggregates.User;
using HiveSpace.UserService.Domain.Exceptions;
using HiveSpace.UserService.Domain.Repositories;

namespace HiveSpace.UserService.Application.Services;

public class UserService : IUserService
{
    private readonly IUserContext _userContext;
    private readonly IUserRepository _userRepository;

    public UserService(
        IUserContext userContext,
        IUserRepository userRepository)
    {
        _userContext = userContext;
        _userRepository = userRepository;
    }

    public async Task<GetUserSettingsResponseDto> GetUserSettingAsync(CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(_userContext.UserId)
            ?? throw new NotFoundException(UserDomainErrorCode.UserNotFound, nameof(User));

        return new GetUserSettingsResponseDto(
            user.Settings.Theme,
            user.Settings.Culture
        );
    }

    public async Task SetUserSettingAsync(
        UpdateUserSettingRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(_userContext.UserId)
            ?? throw new NotFoundException(UserDomainErrorCode.UserNotFound, nameof(User));

        if (request.Theme.HasValue)
            user.UpdateTheme(request.Theme.Value);

        if (request.Culture.HasValue)
            user.UpdateCulture(request.Culture.Value);
        
        await _userRepository.UpdateUserAsync(user, cancellationToken);
    }
}