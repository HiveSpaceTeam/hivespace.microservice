using Asp.Versioning;
using FluentValidation;
using HiveSpace.Core.Helpers;
using HiveSpace.UserService.Application.DTOs.UserAddress;
using HiveSpace.UserService.Application.Interfaces.Services;
using HiveSpace.UserService.Application.Validators.UserAddress;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace HiveSpace.UserService.Api.Controllers;

[Route("api/v{version:apiVersion}/users/address")]
[ApiController]
[ApiVersion("1.0")]
[Authorize(Policy = "RequireUserFullAccessScope")]
public class UserAddressController : ControllerBase
{
    private readonly IUserAddressService _userAddressService;

    public UserAddressController(IUserAddressService userAddressService)
    {
        _userAddressService = userAddressService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<UserAddressDto>), (int)HttpStatusCode.OK)]
    public async Task<IActionResult> GetUserAddress(CancellationToken cancellationToken)
        => Ok(await _userAddressService.GetUserAddressAsync(cancellationToken));

    [HttpPost]
    [ProducesResponseType(typeof(UserAddressDto), (int)HttpStatusCode.OK)]
    public async Task<IActionResult> CreateUserAddress([FromBody] UserAddressRequestDto param, CancellationToken cancellationToken)
    {
        ValidationHelper.ValidateResult(new UserAddressValidator().Validate(param));
        var result = await _userAddressService.CreateUserAddressAsync(param, cancellationToken);
        return CreatedAtAction(nameof(GetUserAddress), new { userAddressId = result.Id }, result);
    }

    [HttpPut("{userAddressId}")]
    [ProducesResponseType((int)HttpStatusCode.NoContent)]
    public async Task<IActionResult> UpdateUserAddress(Guid userAddressId, [FromBody] UserAddressRequestDto param, CancellationToken cancellationToken)
    {
        ValidationHelper.ValidateResult(new UserAddressValidator().Validate(param));
        await _userAddressService.UpdateUserAddressAsync(param, userAddressId, cancellationToken);
        return NoContent();
    }

    [HttpPut("{userAddressId}/default")]
    [ProducesResponseType((int)HttpStatusCode.NoContent)]
    public async Task<IActionResult> SetDefaultUserAddress(Guid userAddressId, CancellationToken cancellationToken)
    {
        await _userAddressService.SetDefaultUserAddressAsync(userAddressId, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{userAddressId}")]
    [ProducesResponseType((int)HttpStatusCode.NoContent)]
    public async Task<IActionResult> DeleteUserAddress(Guid userAddressId, CancellationToken cancellationToken)
    {
        await _userAddressService.DeleteUserAddressAsync(userAddressId, cancellationToken);
        return NoContent();
    }
}
