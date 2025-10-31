using HiveSpace.Core.Helpers;
using HiveSpace.Infrastructure.Authorization;
using HiveSpace.UserService.Application.Interfaces.Services;
using HiveSpace.UserService.Application.Models.Requests.Admin;
using HiveSpace.UserService.Application.Models.Responses.Admin;
using HiveSpace.UserService.Application.Validators.Admin;
using HiveSpace.UserService.Application.Constant.Enum;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HiveSpace.UserService.Api.Controllers;

/// <summary>
/// Administrative operations controller.
/// Handles user management, admin creation, and system administration tasks.
/// </summary>
[ApiController]
[Route("api/v{version:apiVersion}/admins")]
[ApiVersion("1.0")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;

    /// <summary>
    /// Initializes a new instance of the AdminController.
    /// </summary>
    /// <param name="adminService">The admin service for handling administrative operations.</param>
    public AdminController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    /// <summary>
    /// Creates a new admin user.
    /// Only SystemAdmin can create other SystemAdmins.
    /// Admin users can create regular Admins.
    /// </summary>
    [HttpPost]
    [RequireAdmin] // Admin or SystemAdmin required
    public async Task<ActionResult<CreateAdminResponseDto>> CreateAdmin([FromBody] CreateAdminRequestDto request, CancellationToken cancellationToken)
    {
        ValidationHelper.ValidateResult(new CreateAdminValidator().Validate(request));
        var result = await _adminService.CreateAdminAsync(request, cancellationToken);
        return CreatedAtAction(nameof(CreateAdmin), new { id = result.Id }, result);
    }

    /// <summary>
    /// Gets paginated list of users (non-admin users).
    /// Available to Admin and SystemAdmin roles.
    /// </summary>
    [HttpGet("users")]
    [RequireAdmin] // Admin or SystemAdmin required
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

    /// <summary>
    /// Gets paginated list of admin users.
    /// Only SystemAdmin can view other admins.
    /// </summary>
    [HttpGet]
    [RequireAdmin] // Only SystemAdmin can view admin list
    public async Task<ActionResult<GetAdminResponseDto>> GetAdmins(
        [FromQuery] GetAdminRequestDto request,
        CancellationToken cancellationToken)
    {
        ValidationHelper.ValidateResult(new GetAdminValidator().Validate(request));

        var result = await _adminService.GetAdminsAsync(request, cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Updates user status (active/inactive).
    /// Available to Admin and SystemAdmin roles.
    /// </summary>
    [HttpPut("users/status")]
    [RequireAdmin] // Admin or SystemAdmin required
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
