using HiveSpace.IdentityService.Application.Interfaces;
using HiveSpace.Core.Contexts;
using HiveSpace.IdentityService.Domain.Repositories;
using HiveSpace.IdentityService.Domain.Exceptions;
using HiveSpace.IdentityService.Application.Models.Responses;
using HiveSpace.IdentityService.Application.Models.Requests;
using HiveSpace.IdentityService.Domain.Aggregates;

namespace HiveSpace.IdentityService.Application.Services;

public class AddressService : IAddressService
{
    private readonly IUserRepository _userRepository;
    private readonly IUserContext _userContext;

    public AddressService(IUserRepository userRepository, IUserContext userContext)
    {
        _userRepository = userRepository;
        _userContext = userContext;
    }

    public async Task<IEnumerable<AddressResponseDto>> GetAddressesAsync()
    {
        var userId = _userContext.UserId.ToString();
        var user = await _userRepository.GetByIdAsync(userId, includeDetail: true)
            ?? throw new UserNotFoundException();

        var addresses = user.Addresses
            .OrderByDescending(a => a.IsDefault)
            .ThenBy(a => a.CreatedAt);

        return addresses.Select(MapToResponseDto);
    }

    public async Task<AddressResponseDto?> GetAddressAsync(Guid addressId)
    {
        var userId = _userContext.UserId.ToString();
        var user = await _userRepository.GetByIdAsync(userId, includeDetail: true)
            ?? throw new UserNotFoundException();

        var address = user.Addresses.FirstOrDefault(a => a.Id == addressId)
            ?? throw new AddressNotFoundException();

        return MapToResponseDto(address);
    }

    public async Task<AddressResponseDto> CreateAddressAsync(AddressRequestDto createDto)
    {
        var userId = _userContext.UserId.ToString();
        var user = await _userRepository.GetByIdAsync(userId, includeDetail: true)
            ?? throw new UserNotFoundException();

        var addressProps = new AddressProps
        {
            FullName = createDto.FullName,
            Street = createDto.Street,
            Ward = createDto.Ward,
            District = createDto.District,
            Province = createDto.Province,
            Country = createDto.Country,
            ZipCode = createDto.ZipCode,
            PhoneNumber = createDto.PhoneNumber,
        };

        var address = user.AddAddress(addressProps);

        // Handle default logic more efficiently
        if (createDto.IsDefault || !user.Addresses.Any())
        {
            user.SetDefaultAddress(address.Id);
        }
        await _userRepository.SaveChangesAsync();

        return MapToResponseDto(address);
    }

    public async Task<AddressResponseDto> UpdateAddressAsync(Guid addressId, AddressRequestDto updateDto)
    {
        var userId = _userContext.UserId.ToString();
        var user = await _userRepository.GetByIdAsync(userId, includeDetail: true)
            ?? throw new UserNotFoundException();

        var addressProps = new AddressProps
        {
            FullName = updateDto.FullName,
            Street = updateDto.Street,
            Ward = updateDto.Ward,
            District = updateDto.District,
            Province = updateDto.Province,
            Country = updateDto.Country,
            ZipCode = updateDto.ZipCode,
            PhoneNumber = updateDto.PhoneNumber,
        };

        var address = user.UpdateAddress(addressId, addressProps);

        await _userRepository.SaveChangesAsync();
        return MapToResponseDto(address);
    }

    public async Task SetDefaultAddressAsync(Guid addressId)
    {
        var userId = _userContext.UserId.ToString();
        var user = await _userRepository.GetByIdAsync(userId, includeDetail: true)
            ?? throw new UserNotFoundException();

        if (!user.Addresses.Any(a => a.Id == addressId))
            throw new AddressNotFoundException();

        user.SetDefaultAddress(addressId);
        await _userRepository.SaveChangesAsync();
    }

    public async Task DeleteAddressAsync(Guid addressId)
    {
        var userId = _userContext.UserId.ToString();
        var user = await _userRepository.GetByIdAsync(userId, includeDetail: true)
            ?? throw new UserNotFoundException();

        if (!user.Addresses.Any(a => a.Id == addressId))
            throw new AddressNotFoundException();

        user.RemoveAddress(addressId);
        await _userRepository.SaveChangesAsync();
    }

    private static AddressResponseDto MapToResponseDto(Address address)
    {
        return new AddressResponseDto(
            address.Id,
            address.FullName,
            address.Street,
            address.Ward,
            address.District,
            address.Province,
            address.Country,
            address.ZipCode,
            address.PhoneNumber,
            address.IsDefault,
            address.CreatedAt,
            address.UpdatedAt
        );
    }
} 