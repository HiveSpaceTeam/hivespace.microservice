using FluentValidation;
using HiveSpace.Core.Helpers;
using HiveSpace.IdentityService.Application.Interfaces;
using HiveSpace.IdentityService.Application.Models.Requests;
using HiveSpace.IdentityService.Application.Models.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HiveSpace.IdentityService.Application.Controllers;

[Authorize(Policy = "RequireIdentityFullAccessScope")]
[ApiController]
[Route("api/v1/users")]
public class ProfileController : ControllerBase
{
    private readonly IProfileService _profileService;
    private readonly IValidator<SignupRequestDto> _signupValidator;
    private readonly IValidator<UpdateUserRequestDto> _updateUserValidator;
    private readonly IValidator<ChangePasswordRequestDto> _changePasswordValidator;

    public ProfileController(
        IProfileService profileService, 
        IValidator<SignupRequestDto> signupValidator,
        IValidator<UpdateUserRequestDto> updateUserValidator,
        IValidator<ChangePasswordRequestDto> changePasswordValidator)
    {
        _profileService = profileService;
        _signupValidator = signupValidator;
        _updateUserValidator = updateUserValidator;
        _changePasswordValidator = changePasswordValidator;
    }

    /// <summary>
    /// Create a new user account
    /// </summary>
    [HttpPost("signup")]
    public async Task<IActionResult> Signup([FromBody] SignupRequestDto signupDto)
    {
        ValidationHelper.ValidateResult(_signupValidator.Validate(signupDto));
        var result = await _profileService.CreateUserAsync(signupDto);
        return CreatedAtAction(nameof(Signup), new { userId = result.UserId }, result);
    }

    /// <summary>
    /// Update user profile information
    /// </summary>
    [HttpPut("update")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateUserRequestDto updateDto)
    {
        ValidationHelper.ValidateResult(_updateUserValidator.Validate(updateDto));
        await _profileService.UpdateUserInfoAsync(updateDto);
        return NoContent();
    }

    /// <summary>
    /// Change user password
    /// </summary>
    [HttpPut("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestDto changePasswordDto)
    {
        ValidationHelper.ValidateResult(_changePasswordValidator.Validate(changePasswordDto));
        await _profileService.ChangePassword(changePasswordDto);
        return NoContent();
    }
} 