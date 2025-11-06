using HiveSpace.Core.Helpers;
using HiveSpace.Infrastructure.Authorization;
using HiveSpace.UserService.Application.Interfaces.Services;
using HiveSpace.UserService.Application.Models.Requests.User;
using HiveSpace.UserService.Application.Models.Responses.User;
using HiveSpace.UserService.Application.Validators.User;
using Microsoft.AspNetCore.Mvc;

namespace HiveSpace.UserService.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/users")]
[ApiVersion("1.0")]
[RequireAdminOrUser]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    /// <summary>
    /// Get the current user's settings
    /// </summary>
    [HttpGet("settings")]
    [ProducesResponseType(typeof(GetUserSettingsResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GetUserSettingsResponseDto>> GetUserSetting(CancellationToken cancellationToken)
    {
        var result = await _userService.GetUserSettingAsync(cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Update the current user's settings
    /// </summary>
    [HttpPut("settings")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> SetUserSetting(
        [FromBody] UpdateUserSettingRequestDto request,
        CancellationToken cancellationToken)
    {
        ValidationHelper.ValidateResult(new UpdateUserSettingValidator().Validate(request));
        await _userService.SetUserSettingAsync(request, cancellationToken);
        return NoContent();
    }
}