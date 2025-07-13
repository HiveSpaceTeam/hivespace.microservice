using HiveSpace.Core.UserContext;
using HiveSpace.IdentityService.Application.Interfaces;
using HiveSpace.IdentityService.Application.Models.Requests;
using HiveSpace.IdentityService.Application.Models.Responses;
using HiveSpace.IdentityService.Domain.Aggregates;
using HiveSpace.IdentityService.Domain.Aggregates.Enums;
using HiveSpace.IdentityService.Domain.Exceptions;
using HiveSpace.IdentityService.Domain.Repositories;
using Microsoft.AspNetCore.Identity;

namespace HiveSpace.IdentityService.Application.Services;

public class ProfileService : IProfileService
{
    private readonly IUserRepository _userRepository;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUserContext _userContext;

    public ProfileService(
        IUserRepository userRepository,
        UserManager<ApplicationUser> userManager,
        IUserContext userContext)
    {
        _userRepository = userRepository;
        _userManager = userManager;
        _userContext = userContext;
    }

    public async Task<SignupResponseDto> CreateUserAsync(SignupRequestDto requestDto)
    {
        // Check if email already exists
        if (await _userRepository.IsEmailExistsAsync(requestDto.Email))
        {
            throw new EmailAlreadyExistsException();
        }

        // Check if username already exists
        if (await _userRepository.IsUserNameExistsAsync(requestDto.UserName))
        {
            throw new UserAlreadyExistsException();
        }


        var user = new ApplicationUser(requestDto.Email, requestDto.UserName, requestDto.FullName, null, null, null);

        var result = await _userManager.CreateAsync(user, requestDto.Password);
        if (!result.Succeeded)
        {
            // Check for specific password-related errors
            if (result.Errors.Any(e => e.Code == "PasswordTooShort" || e.Code == "PasswordRequiresNonAlphanumeric" || 
                                       e.Code == "PasswordRequiresDigit" || e.Code == "PasswordRequiresLower" || 
                                       e.Code == "PasswordRequiresUpper"))
            {
                throw new PasswordTooWeakException();
            }
            
            // For other identity errors, throw a generic invalid password exception
            throw new InvalidPasswordException();
        }

        await _userRepository.SaveChangesAsync();

        return new SignupResponseDto
        {
            UserId = Guid.Parse(user.Id),
            Email = user.Email!,
            FullName = user.FullName ?? string.Empty,
            UserName = user.UserName!
        };
    }

    public async Task UpdateUserInfoAsync(UpdateUserRequestDto param)
    {
        var userId = _userContext.UserId.ToString();
        var user = await _userRepository.GetByIdAsync(userId)
            ?? throw new UserNotFoundException();

        user.UpdateUserInfo(
            param.UserName,
            param.FullName,
            param.Email,
            param.PhoneNumber,
            param.Gender,
            param.DateOfBirth);

        await _userRepository.SaveChangesAsync();
    }

    public async Task ChangePassword(ChangePasswordRequestDto requestDto)
    {
        var userId = _userContext.UserId.ToString();
        var user = await _userRepository.GetByIdAsync(userId)
            ?? throw new UserNotFoundException();

        var result = await _userManager.ChangePasswordAsync(user, requestDto.Password, requestDto.NewPassword);
        if (!result.Succeeded)
        {
            // Check for specific password-related errors
            if (result.Errors.Any(e => e.Code == "PasswordTooShort" || e.Code == "PasswordRequiresNonAlphanumeric" || 
                                       e.Code == "PasswordRequiresDigit" || e.Code == "PasswordRequiresLower" || 
                                       e.Code == "PasswordRequiresUpper"))
            {
                throw new PasswordTooWeakException();
            }
            
            // For other identity errors, throw a generic invalid password exception
            throw new InvalidPasswordException();
        }
    }
}
