using HiveSpace.Core.Contexts;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.UserService.Application.DTOs.UserAddress;
using HiveSpace.UserService.Application.Interfaces.Services;
using HiveSpace.UserService.Domain.Aggregates.User;
using HiveSpace.UserService.Domain.Enums;
using HiveSpace.UserService.Domain.Exceptions;
using HiveSpace.UserService.Domain.Repositories;

namespace HiveSpace.UserService.Application.Services;

public class UserAddressService : IUserAddressService
{
    private readonly IUserContext _userContext;
    private readonly IUserRepository _userRepository;

    public UserAddressService(
        IUserContext userContext,
        IUserRepository userRepository)
    {
        _userContext = userContext;
        _userRepository = userRepository;
    }

    public async Task<List<UserAddressDto>> GetUserAddressAsync(CancellationToken cancellationToken = default)
    {
        var userId = _userContext.UserId;
        var user = await _userRepository.GetByIdAsync(userId, includeDetail: true);
            
        if (user == null)
            throw new NotFoundException(UserDomainErrorCode.UserNotFound, nameof(User));

        return user.Addresses.Select(MapToDto).ToList();
    }

    public async Task<UserAddressDto> CreateUserAddressAsync(UserAddressRequestDto param, CancellationToken cancellationToken = default)
    {
        var userId = _userContext.UserId;
        var user = await _userRepository.GetByIdAsync(userId, includeDetail: true);

        if (user == null)
            throw new NotFoundException(UserDomainErrorCode.UserNotFound, nameof(User));

        user.AddAddress(
            param.FullName,
            param.PhoneNumber,
            param.Street,
            param.District,
            param.Province,
            param.Country,
            param.ZipCode,
            param.AddressType,
            param.IsDefault
        );

        await _userRepository.UpdateUserAddressesAsync(user, cancellationToken);

        // Return the newly created address mapped to DTO
        var createdAddress = user.Addresses.Last();
        return MapToDto(createdAddress);
    }

    public async Task UpdateUserAddressAsync(UserAddressRequestDto param, Guid userAddressId, CancellationToken cancellationToken = default)
    {
        var userId = _userContext.UserId;
        var user = await _userRepository.GetByIdAsync(userId, includeDetail: true);

        if (user == null)
            throw new NotFoundException(UserDomainErrorCode.UserNotFound, nameof(User));

        user.UpdateAddress(
            userAddressId,
            param.FullName,
            param.PhoneNumber,
            param.Street,
            param.District,
            param.Province,
            param.Country,
            param.ZipCode,
            param.AddressType
        );

        if (param.IsDefault)
        {
            user.MarkAddressAsDefault(userAddressId);
        }

        await _userRepository.UpdateUserAddressesAsync(user, cancellationToken);
    }

    public async Task SetDefaultUserAddressAsync(Guid userAddressId, CancellationToken cancellationToken = default)
    {
        var userId = _userContext.UserId;
        var user = await _userRepository.GetByIdAsync(userId, includeDetail: true);

        if (user == null)
            throw new NotFoundException(UserDomainErrorCode.UserNotFound, nameof(User));

        user.MarkAddressAsDefault(userAddressId);
        await _userRepository.UpdateUserAddressesAsync(user, cancellationToken);
    }

    public async Task DeleteUserAddressAsync(Guid userAddressId, CancellationToken cancellationToken = default)
    {
        var userId = _userContext.UserId;
        var user = await _userRepository.GetByIdAsync(userId, includeDetail: true);

        if (user == null)
            throw new NotFoundException(UserDomainErrorCode.UserNotFound, nameof(User));

        user.RemoveAddress(userAddressId);
        await _userRepository.UpdateUserAddressesAsync(user, cancellationToken);
    }

    private static UserAddressDto MapToDto(Address address)
    {
        return new UserAddressDto(
            address.Id,
            address.FullName,
            address.PhoneNumber,
            address.Street,
            address.District,
            address.Province,
            address.Country,
            address.ZipCode,
            address.AddressType,
            address.IsDefault
        );
    }
}
