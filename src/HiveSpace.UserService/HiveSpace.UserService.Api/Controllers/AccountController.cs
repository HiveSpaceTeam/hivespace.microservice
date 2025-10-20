using HiveSpace.Core.Helpers;
using HiveSpace.Infrastructure.Authorization;
using HiveSpace.UserService.Application.Interfaces.Services;
using HiveSpace.UserService.Application.Models.Requests.Account;
using HiveSpace.UserService.Application.Validators.Account;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HiveSpace.UserService.Api.Controllers;

/// <summary>
/// Controller for account-related operations such as sending email verification links
/// and confirming email verification tokens.
/// </summary>
[ApiController]
[Route("api/v{version:apiVersion}/accounts")]
[ApiVersion("1.0")]
public class AccountController : ControllerBase
{
    private readonly IAccountService _accountService;

    /// <summary>
    /// Initializes a new instance of the AccountController.
    /// </summary>
    /// <param name="accountService">The account service for handling account operations.</param>
    public AccountController(IAccountService accountService)
    {
        _accountService = accountService;
    }

    /// <summary>
    /// Send email verification link to the authenticated user.
    /// Available to all authenticated users (Admin, Seller, Customer).
    /// </summary>
    /// <param name="request">Email verification request details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>No content on successful send</returns>
    [HttpPost("send-verification-email")]
    [RequireAdminOrUser] // Available to both admins and users
    public async Task<IActionResult> SendEmailVerification(
        [FromBody] SendEmailVerificationRequestDto request, 
        CancellationToken cancellationToken)
    {
        ValidationHelper.ValidateResult(new SendEmailVerificationValidator().Validate(request));
        await _accountService.SendEmailVerificationAsync(request, cancellationToken);
        return Accepted();
    }

    /// <summary>
    /// Alternative endpoint for email verification via GET (for email links).
    /// This allows users to verify email by clicking a link in their email.
    /// Available to all authenticated users (Admin, Seller, Customer).
    /// </summary>
    /// <param name="token">Verification token from the email link</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>No content on successful verification or redirect</returns>
    [HttpGet("confirm-email")]
    [RequireAdminOrUser] // Available to both admins and users
    // [AllowAnonymous] // Allow anonymous access for email link verification
    public async Task<ActionResult> VerifyEmailFromLink(
        [FromQuery] string token,
        CancellationToken cancellationToken = default)
    {
        await _accountService.VerifyEmailAsync(token, cancellationToken);
        return NoContent();
    }
}