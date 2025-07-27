using FluentValidation;
using HiveSpace.Core.Helpers;
using HiveSpace.IdentityService.Application.Interfaces;
using HiveSpace.IdentityService.Application.Models.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HiveSpace.IdentityService.Application.Controllers;

[Authorize(Policy = "RequireIdentityFullAccessScope")]
[ApiController]
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/users")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IValidator<SignupRequestDto> _signupValidator;
    private readonly IValidator<UpdateUserRequestDto> _updateUserValidator;
    private readonly IValidator<ChangePasswordRequestDto> _changePasswordValidator;

    public UserController(
        IUserService userService, 
        IValidator<SignupRequestDto> signupValidator,
        IValidator<UpdateUserRequestDto> updateUserValidator,
        IValidator<ChangePasswordRequestDto> changePasswordValidator)
    {
        _userService = userService;
        _signupValidator = signupValidator;
        _updateUserValidator = updateUserValidator;
        _changePasswordValidator = changePasswordValidator;
    }

    /// <summary>
    /// Create a new user account
    /// </summary>
    [HttpPost("signup")]
    [AllowAnonymous]
    public async Task<IActionResult> Signup([FromBody] SignupRequestDto signupDto)
    {
        ValidationHelper.ValidateResult(_signupValidator.Validate(signupDto));
        var result = await _userService.CreateUserAsync(signupDto);
        return CreatedAtAction(nameof(Signup), new { userId = result.UserId }, result);
    }

    /// <summary>
    /// Update user information
    /// </summary>
    [HttpPut()]
    public async Task<IActionResult> UpdateUser([FromBody] UpdateUserRequestDto updateDto)
    {
        ValidationHelper.ValidateResult(_updateUserValidator.Validate(updateDto));
        await _userService.UpdateUserInfoAsync(updateDto);
        return NoContent();
    }

    /// <summary>
    /// Change user password
    /// </summary>
    [HttpPut("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestDto changePasswordDto)
    {
        ValidationHelper.ValidateResult(_changePasswordValidator.Validate(changePasswordDto));
        await _userService.ChangePassword(changePasswordDto);
        return NoContent();
    }
} 