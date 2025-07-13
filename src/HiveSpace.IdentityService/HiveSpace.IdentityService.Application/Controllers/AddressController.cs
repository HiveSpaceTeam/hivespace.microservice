using FluentValidation;
using HiveSpace.Core.Helpers;
using HiveSpace.IdentityService.Application.Interfaces;
using HiveSpace.IdentityService.Application.Models.Requests;
using HiveSpace.IdentityService.Application.Models.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HiveSpace.IdentityService.Application.Controllers;

[Authorize(Policy = "RequireIdentityFullAccessScope")]
[ApiController]
[Route("api/v1/users/address")]
public class AddressController : ControllerBase
{
    private readonly IAddressService _addressService;
    private readonly IValidator<AddressRequestDto> _addressValidator;

    public AddressController(IAddressService addressService, IValidator<AddressRequestDto> createValidator)
    {
        _addressService = addressService;
        _addressValidator = createValidator;
    }

    /// <summary>
    /// Get all addresses for the current user
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAddresses()
    {
        var addresses = await _addressService.GetAddressesAsync();
        return Ok(addresses);
    }

    /// <summary>
    /// Get a specific address by ID
    /// </summary>
    [HttpGet("{addressId:guid}")]
    public async Task<IActionResult> GetAddress(Guid addressId)
    {
        var address = await _addressService.GetAddressAsync(addressId);
        return Ok(address);
    }

    /// <summary>
    /// Create a new address for the current user
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateAddress([FromBody] AddressRequestDto createDto)
    {
        ValidationHelper.ValidateResult(_addressValidator.Validate(createDto));
        var address = await _addressService.CreateAddressAsync(createDto);
        return CreatedAtAction(nameof(GetAddress), new { addressId = address.Id }, address.Id);
    }

    /// <summary>
    /// Update an existing address
    /// </summary>
    [HttpPut("{addressId:guid}")]
    public async Task<IActionResult> UpdateAddress(Guid addressId, [FromBody] AddressRequestDto updateDto)
    {
        ValidationHelper.ValidateResult(_addressValidator.Validate(updateDto));
        var address = await _addressService.UpdateAddressAsync(addressId, updateDto);
        return Ok(address);
    }

    /// <summary>
    /// Set an address as default
    /// </summary>
    [HttpPut("{addressId:guid}/default")]
    public async Task<IActionResult> SetDefaultAddress(Guid addressId)
    {
        await _addressService.SetDefaultAddressAsync(addressId);
        return NoContent();
    }

    /// <summary>
    /// Delete an address
    /// </summary>
    [HttpDelete("{addressId:guid}")]
    public async Task<IActionResult> DeleteAddress(Guid addressId)
    {
        await _addressService.DeleteAddressAsync(addressId);
        return NoContent();
    }
} 