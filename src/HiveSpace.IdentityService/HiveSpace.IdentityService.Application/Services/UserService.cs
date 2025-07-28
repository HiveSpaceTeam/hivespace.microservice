using HiveSpace.Core.Contexts;
using HiveSpace.IdentityService.Application.Interfaces;
using HiveSpace.IdentityService.Application.Models.Requests;
using HiveSpace.IdentityService.Application.Models.Responses;
using HiveSpace.IdentityService.Application.Constants;
using HiveSpace.IdentityService.Domain.Aggregates;
using HiveSpace.IdentityService.Domain.Exceptions;
using HiveSpace.IdentityService.Domain.Repositories;
using Microsoft.AspNetCore.Identity;
using HiveSpace.Infrastructure.Persistence.Transaction;

namespace HiveSpace.IdentityService.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUserContext _userContext;
    private readonly ITransactionService _transactionService;

    public UserService(
        IUserRepository userRepository,
        UserManager<ApplicationUser> userManager,
        IUserContext userContext,
        ITransactionService transactionService)
    {
        _userRepository = userRepository;
        _userManager = userManager;
        _userContext = userContext;
        _transactionService = transactionService;
    }

    /// <summary>
    /// Creates a new user account.
    /// </summary>
    /// <param name="requestDto">Signup request data.</param>
    /// <returns>Signup response DTO.</returns>
    /// <exception cref="EmailAlreadyExistsException"></exception>
    /// <exception cref="UserAlreadyExistsException"></exception>
    /// <exception cref="PasswordTooWeakException"></exception>
    /// <exception cref="InvalidPasswordException"></exception>
    public async Task<SignupResponseDto> CreateUserAsync(SignupRequestDto requestDto)
    {
        SignupResponseDto resultDto = default!;
        await _transactionService.InTransactionScopeAsync(async (transaction) =>
        {
            if (await _userRepository.IsEmailExistsAsync(requestDto.Email))
                throw new EmailAlreadyExistsException();

            if (await _userRepository.IsUserNameExistsAsync(requestDto.UserName))
                throw new UserAlreadyExistsException();

            var user = new ApplicationUser(requestDto.Email, requestDto.UserName, requestDto.FullName, null, null, null);
            var result = await _userManager.CreateAsync(user, requestDto.Password);
            HandlePasswordErrors(result);

            await _userRepository.SaveChangesAsync();

            resultDto = new SignupResponseDto(
                user.Email ?? string.Empty,
                user.FullName ?? string.Empty,
                user.UserName ?? string.Empty,
                Guid.Parse(user.Id)
            );
        }, true, nameof(CreateUserAsync));
        
        return resultDto;
    }

    /// <summary>
    /// Updates the current user's information.
    /// </summary>
    /// <param name="requestDto">Update user request data.</param>
    /// <exception cref="UserNotFoundException"></exception>
    public async Task UpdateUserInfoAsync(UpdateUserRequestDto requestDto)
    {
        var userId = _userContext.UserId.ToString();
        var user = await _userRepository.GetByIdAsync(userId)
            ?? throw new UserNotFoundException();

        user.UpdateUserInfo(
            requestDto.UserName,
            requestDto.FullName,
            requestDto.Email,
            requestDto.PhoneNumber,
            requestDto.Gender,
            requestDto.DateOfBirth);

        await _userRepository.SaveChangesAsync();
    }

    /// <summary>
    /// Changes the current user's password.
    /// </summary>
    /// <param name="requestDto">Change password request data.</param>
    /// <exception cref="UserNotFoundException"></exception>
    /// <exception cref="PasswordTooWeakException"></exception>
    /// <exception cref="InvalidPasswordException"></exception>
    public async Task ChangePassword(ChangePasswordRequestDto requestDto)
    {
        var userId = _userContext.UserId.ToString();
        var user = await _userRepository.GetByIdAsync(userId)
            ?? throw new UserNotFoundException();

        var result = await _userManager.ChangePasswordAsync(user, requestDto.Password, requestDto.NewPassword);
        HandlePasswordErrors(result);
    }

    /// <summary>
    /// Handles password-related errors from IdentityResult.
    /// Throws PasswordTooWeakException or InvalidPasswordException as appropriate.
    /// </summary>
    /// <param name="result">The IdentityResult to check.</param>
    /// <exception cref="PasswordTooWeakException"></exception>
    /// <exception cref="InvalidPasswordException"></exception>
    private static void HandlePasswordErrors(IdentityResult result)
    {
        if (result.Succeeded) return;
        if (result.Errors.Any(e => e.Code == IdentityResultError.PasswordTooShort || e.Code == IdentityResultError.PasswordRequiresNonAlphanumeric ||
                                   e.Code == IdentityResultError.PasswordRequiresDigit || e.Code == IdentityResultError.PasswordRequiresLower ||
                                   e.Code == IdentityResultError.PasswordRequiresUpper))
        {
            throw new PasswordTooWeakException();
        }
        throw new InvalidPasswordException();
    }
}
