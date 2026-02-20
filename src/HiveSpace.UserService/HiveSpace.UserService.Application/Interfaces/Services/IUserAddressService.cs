using HiveSpace.UserService.Application.DTOs.UserAddress;
using HiveSpace.UserService.Domain.Aggregates.User;

namespace HiveSpace.UserService.Application.Interfaces.Services;

public interface IUserAddressService
{
    Task<List<UserAddressDto>> GetUserAddressAsync(CancellationToken cancellationToken = default);
    Task<UserAddressDto> CreateUserAddressAsync(UserAddressRequestDto param, CancellationToken cancellationToken = default);
    Task UpdateUserAddressAsync(UserAddressRequestDto param, Guid userAddressId, CancellationToken cancellationToken = default);
    Task SetDefaultUserAddressAsync(Guid userAddressId, CancellationToken cancellationToken = default);
    Task DeleteUserAddressAsync(Guid userAddressId, CancellationToken cancellationToken = default);
}
