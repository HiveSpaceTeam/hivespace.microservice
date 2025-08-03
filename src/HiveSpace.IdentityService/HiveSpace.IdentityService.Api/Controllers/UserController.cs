using FluentValidation;
using HiveSpace.Core.Helpers;
//using HiveSpace.IdentityService.Api.Models.Requests;
using HiveSpace.IdentityService.Application.Commands.Users;
using HiveSpace.IdentityService.Application.Models.Requests;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HiveSpace.IdentityService.Api.Controllers;

[Authorize(Policy = "RequireIdentityFullAccessScope")]
[ApiController]
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/users")]
public class UserController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IValidator<SignupRequestDto> _signupValidator;
    private readonly IValidator<UpdateUserRequestDto> _updateUserValidator;
    private readonly IValidator<ChangePasswordRequestDto> _changePasswordValidator;

    public UserController(
        IMediator mediator,
        IValidator<SignupRequestDto> signupValidator,
        IValidator<UpdateUserRequestDto> updateUserValidator,
        IValidator<ChangePasswordRequestDto> changePasswordValidator)
    {
        _mediator = mediator;
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
        var command = CreateUserCommand.FromDto(signupDto);
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(Signup), new { userId = result.UserId }, result);
    }

    /// <summary>
    /// Update user information
    /// </summary>
    [HttpPut()]
    public async Task<IActionResult> UpdateUser([FromBody] UpdateUserRequestDto updateDto)
    {
        ValidationHelper.ValidateResult(_updateUserValidator.Validate(updateDto));
        var command = UpdateUserInfoCommand.FromDto(updateDto);
        await _mediator.Send(command);
        return NoContent();
    }

    /// <summary>
    /// Change user password
    /// </summary>
    [HttpPut("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestDto changePasswordDto)
    {
        ValidationHelper.ValidateResult(_changePasswordValidator.Validate(changePasswordDto));
        var command = ChangePasswordCommand.FromDto(changePasswordDto);
        await _mediator.Send(command);
        return NoContent();
    }
} 