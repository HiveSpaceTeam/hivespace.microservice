using FluentValidation;
using HiveSpace.Core.Helpers;
using HiveSpace.IdentityService.Application.Commands.Addresses;
using HiveSpace.IdentityService.Application.Models.Requests;
using HiveSpace.IdentityService.Application.Queries.Addresses;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HiveSpace.IdentityService.Api.Controllers;

[Authorize(Policy = "RequireIdentityFullAccessScope")]
[ApiController]
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/users/address")]
public class AddressController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IValidator<AddressRequestDto> _addressValidator;

    public AddressController(IMediator mediator, IValidator<AddressRequestDto> createValidator)
    {
        _mediator = mediator;
        _addressValidator = createValidator;
    }

    /// <summary>
    /// Get all addresses for the current user
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAddresses()
    {
        var query = new GetAddressesQuery();
        var addresses = await _mediator.Send(query);
        return Ok(addresses);
    }

    /// <summary>
    /// Get a specific address by ID
    /// </summary>
    [HttpGet("{addressId:guid}")]
    public async Task<IActionResult> GetAddress(Guid addressId)
    {
        var query = new GetAddressQuery(addressId);
        var address = await _mediator.Send(query);
        return Ok(address);
    }

    /// <summary>
    /// Create a new address for the current user
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateAddress([FromBody] AddressRequestDto createDto)
    {
        ValidationHelper.ValidateResult(_addressValidator.Validate(createDto));
        var command = CreateAddressCommand.FromDto(createDto);
        var address = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetAddress), new { addressId = address.Id }, address);
    }

    /// <summary>
    /// Update an existing address
    /// </summary>
    [HttpPut("{addressId:guid}")]
    public async Task<IActionResult> UpdateAddress(Guid addressId, [FromBody] AddressRequestDto updateDto)
    {
        ValidationHelper.ValidateResult(_addressValidator.Validate(updateDto));
        var command = UpdateAddressCommand.FromDto(addressId, updateDto);
        var address = await _mediator.Send(command);
        return Ok(address);
    }

    /// <summary>
    /// Set an address as default
    /// </summary>
    [HttpPut("{addressId:guid}/default")]
    public async Task<IActionResult> SetDefaultAddress(Guid addressId)
    {
        var command = new SetDefaultAddressCommand(addressId);
        await _mediator.Send(command);
        return NoContent();
    }

    /// <summary>
    /// Delete an address
    /// </summary>
    [HttpDelete("{addressId:guid}")]
    public async Task<IActionResult> DeleteAddress(Guid addressId)
    {
        var command = new DeleteAddressCommand(addressId);
        await _mediator.Send(command);
        return NoContent();
    }
}