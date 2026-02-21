using HiveSpace.Core.Contexts;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.UserService.Application.DTOs.UserAddress;
using HiveSpace.UserService.Application.Interfaces.Services;
using HiveSpace.UserService.Domain.Aggregates.User;
using HiveSpace.UserService.Domain.Enums;
using HiveSpace.UserService.Domain.Exceptions;
using HiveSpace.UserService.Domain.Repositories;

namespace HiveSpace.UserService.Application.Services;

public class UserAddressService(IUserContext userContext, IUserRepository userRepository) : IUserAddressService
{
    public async Task<List<UserAddressDto>> GetUserAddressAsync(CancellationToken cancellationToken = default)
    {
        var userId = userContext.UserId;
        var user = await userRepository.GetByIdAsync(userId, includeDetail: true);
            
        if (user == null)
            throw new NotFoundException(UserDomainErrorCode.UserNotFound, nameof(User));

        return user.Addresses.Select(MapToDto).ToList();
    }

    public async Task<UserAddressDto> CreateUserAddressAsync(UserAddressRequestDto param, CancellationToken cancellationToken = default)
    {
        var userId = userContext.UserId;
        var user = await userRepository.GetByIdAsync(userId, includeDetail: true) 
            ?? throw new NotFoundException(UserDomainErrorCode.UserNotFound, nameof(User));

        var createdAddress = user.AddAddress(
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

        await userRepository.UpdateUserAddressesAsync(user, cancellationToken);

        return MapToDto(createdAddress);
    }

    public async Task UpdateUserAddressAsync(UserAddressRequestDto param, Guid userAddressId, CancellationToken cancellationToken = default)
    {
        var userId = userContext.UserId;
        var user = await userRepository.GetByIdAsync(userId, includeDetail: true)
            ?? throw new NotFoundException(UserDomainErrorCode.UserNotFound, nameof(User));

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

        await userRepository.UpdateUserAddressesAsync(user, cancellationToken);
    }

    public async Task SetDefaultUserAddressAsync(Guid userAddressId, CancellationToken cancellationToken = default)
    {
        var userId = userContext.UserId;
        var user = await userRepository.GetByIdAsync(userId, includeDetail: true)
            ?? throw new NotFoundException(UserDomainErrorCode.UserNotFound, nameof(User));

        user.MarkAddressAsDefault(userAddressId);
        await userRepository.UpdateUserAddressesAsync(user, cancellationToken);
    }

    public async Task DeleteUserAddressAsync(Guid userAddressId, CancellationToken cancellationToken = default)
    {
        var userId = userContext.UserId;
        var user = await userRepository.GetByIdAsync(userId, includeDetail: true)
            ?? throw new NotFoundException(UserDomainErrorCode.UserNotFound, nameof(User));

        user.RemoveAddress(userAddressId);
        await userRepository.UpdateUserAddressesAsync(user, cancellationToken);
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
