using HiveSpace.Core.Helpers;
using HiveSpace.UserService.Application.Interfaces.Services;
using HiveSpace.UserService.Application.Models.Requests.Admin;
using HiveSpace.UserService.Application.Models.Responses.Admin;
using HiveSpace.UserService.Application.Validators.Admin;
using HiveSpace.UserService.Application.Constant.Enum;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HiveSpace.UserService.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/admins")]
[ApiVersion("1.0")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;

    public AdminController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    [HttpPost]
    [Authorize(Policy = "RequireUserFullAccessScope")] // ensure authenticated scope
    public async Task<ActionResult<CreateAdminResponseDto>> CreateAdmin([FromBody] CreateAdminRequestDto request, CancellationToken cancellationToken)
    {
        ValidationHelper.ValidateResult(new CreateAdminValidator().Validate(request));
        var result = await _adminService.CreateAdminAsync(request, cancellationToken);
        return CreatedAtAction(nameof(CreateAdmin), new { id = result.Id }, result);
    }

    [HttpGet("users")]
    [Authorize(Policy = "RequireUserFullAccessScope")]
    public async Task<ActionResult<GetUsersResponseDto>> GetUsers(
        [FromQuery] GetUsersRequestDto request,
        CancellationToken cancellationToken)
    {
        // Validate request
        ValidationHelper.ValidateResult(new GetUsersValidator().Validate(request));

        // Call service
        var result = await _adminService.GetUsersAsync(request, cancellationToken);

        return Ok(result);
    }

    [HttpGet]
    [Authorize(Policy = "RequireUserFullAccessScope")]
    public async Task<ActionResult<GetAdminResponseDto>> GetAdmins(
        [FromQuery] GetAdminRequestDto request,
        CancellationToken cancellationToken)
    {
        ValidationHelper.ValidateResult(new GetAdminValidator().Validate(request));

        var result = await _adminService.GetAdminsAsync(request, cancellationToken);

        return Ok(result);
    }

    [HttpPut("users/status")]
    [Authorize(Policy = "RequireUserFullAccessScope")]
    public async Task<ActionResult<SetStatusResponseDto>> SetUserStatus(
        [FromBody] SetUserStatusRequestDto request,
        CancellationToken cancellationToken)
    {
        // Validate request
        ValidationHelper.ValidateResult(new SetUserStatusValidator().Validate(request));

        // Call service method - returns strongly-typed DTO based on ResponseType
        var result = await _adminService.SetUserStatusAsync(request, cancellationToken);

        return Ok(result);
    }

    [HttpDelete("users/{userId}")]
    [Authorize(Policy = "RequireUserFullAccessScope")]
    public async Task<ActionResult<DeleteUserResponseDto>> DeleteUser(
        Guid userId,
        CancellationToken cancellationToken)
    {
        // Call service method
        var result = await _adminService.DeleteUserAsync(userId, cancellationToken);

        return Ok(result);
    }
}
