using HiveSpace.IdentityService.Application.Models.Requests;
using HiveSpace.IdentityService.Application.Models.Responses;

namespace HiveSpace.IdentityService.Application.Interfaces;

public interface IAddressService
{
    Task<IEnumerable<AddressResponseDto>> GetAddressesAsync();
    Task<AddressResponseDto?> GetAddressAsync(Guid addressId);
    Task<AddressResponseDto> CreateAddressAsync(AddressRequestDto createDto);
    Task<AddressResponseDto> UpdateAddressAsync(Guid addressId, AddressRequestDto updateDto);
    Task SetDefaultAddressAsync(Guid addressId);
    Task DeleteAddressAsync(Guid addressId);
} 