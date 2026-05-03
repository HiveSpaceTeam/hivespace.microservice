using HiveSpace.Core.Contexts;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.UserService.Application.DTOs.User;
using HiveSpace.UserService.Application.Interfaces.Services;
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
        var user = await _userRepository.GetByIdAsync(_userContext.UserId, cancellationToken: cancellationToken)
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
        var user = await _userRepository.GetByIdAsync(_userContext.UserId, cancellationToken: cancellationToken)
            ?? throw new NotFoundException(UserDomainErrorCode.UserNotFound, nameof(User));

        if (request.Theme.HasValue)
            user.UpdateTheme(request.Theme.Value);

        if (request.Culture.HasValue)
            user.UpdateCulture(request.Culture.Value);

        await _userRepository.UpdateUserAsync(user, cancellationToken);
    }

    public async Task<GetUserProfileResponseDto> GetUserProfileAsync(CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(_userContext.UserId, cancellationToken: cancellationToken)
            ?? throw new NotFoundException(UserDomainErrorCode.UserNotFound, nameof(User));

        return new GetUserProfileResponseDto(
            user.FullName,
            user.UserName,
            user.Email.Value,
            user.AvatarUrl,
            user.PhoneNumber?.Value,
            user.Gender,
            user.DateOfBirth?.Value
        );
    }

    public async Task UpdateUserProfileAsync(UpdateUserProfileRequestDto request, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(_userContext.UserId, cancellationToken: cancellationToken)
            ?? throw new NotFoundException(UserDomainErrorCode.UserNotFound, nameof(User));

        if (request.UserName != null)
        {
            // Non-atomic check: two concurrent requests with the same userName can both pass.
            // A unique index on the userName column is required for hard enforcement.
            var existing = await _userRepository.GetByUserNameAsync(request.UserName, cancellationToken);
            if (existing is not null && existing.Id != user.Id)
                throw new ConflictException(UserDomainErrorCode.UserNameAlreadyExists, nameof(User.UserName));
        }

        var phoneNumber = request.PhoneNumber != null ? PhoneNumber.Create(request.PhoneNumber) : null;
        var dateOfBirth = request.DateOfBirth.HasValue ? DateOfBirth.Create(request.DateOfBirth.Value) : null;

        user.UpdateProfile(request.FullName, phoneNumber, dateOfBirth, request.Gender, request.UserName);

        await _userRepository.UpdateUserAsync(user, cancellationToken);
    }
}
