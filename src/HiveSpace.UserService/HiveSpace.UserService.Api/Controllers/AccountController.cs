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
[Produces("application/json")]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
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
    /// Request an email verification link.
    /// </summary>
    /// <param name="request">Email verification request with callback URL</param>
    /// <param name="cancellationToken">Optional cancellation token</param>
    /// <returns>Accepted (202) when verification email is sent</returns>
    /// <response code="202">Email verification link sent successfully</response>
    /// <response code="400">Invalid request data</response>
    /// <response code="409">Email already verified</response>
    [HttpPost("email-verification")]
    [RequireAdminOrUser]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> RequestEmailVerification(
        [FromBody] SendEmailVerificationRequestDto request, 
        CancellationToken cancellationToken = default)
    {
        ValidationHelper.ValidateResult(new SendEmailVerificationValidator().Validate(request));
        await _accountService.SendEmailVerificationAsync(request, cancellationToken);
        return Accepted();
    }

    /// <summary>
    /// Verify email address using verification token.
    /// </summary>
    /// <param name="request">Verification request containing the token</param>
    /// <param name="cancellationToken">Optional cancellation token</param>
    /// <returns>
    /// - Redirect (302) to returnUrl if provided
    /// - NoContent (204) if successful without returnUrl
    /// </returns>
    /// <response code="204">Email verified successfully</response>
    /// <response code="302">Email verified and redirecting to return URL</response>
    /// <response code="400">Invalid verification token</response>
    /// <response code="409">Email already verified</response>
    [HttpPost("email-verification/verify")]
    [RequireAdminOrUser]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status302Found)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult> VerifyEmail(
        ConfirmEmailVerificationRequestDto request,
        CancellationToken cancellationToken = default)
    {
        await _accountService.ConfirmEmailVerificationAsync(request, cancellationToken);

        // If returnUrl is provided, redirect after successful verification
        if (!string.IsNullOrEmpty(request.ReturnUrl))
        {
            return Redirect(request.ReturnUrl);
        }
        
        return NoContent();
    }
}