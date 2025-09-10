using HiveSpace.Core.Helpers;
using HiveSpace.UserService.Application.Models.Requests.Admin;
using HiveSpace.UserService.Application.Models.Responses.Admin;
using HiveSpace.UserService.Application.Validators.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HiveSpace.UserService.Application.Interface;

namespace HiveSpace.UserService.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/admin")]
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
}
