using HiveSpace.Core.Contexts;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.UserService.Application.UserAddresses.Dtos;
using HiveSpace.UserService.Domain.Aggregates.User;
using HiveSpace.UserService.Domain.Exceptions;
using HiveSpace.UserService.Domain.Repositories;
using MediatR;

namespace HiveSpace.UserService.Application.UserAddresses.Queries.GetDefaultUserAddress;

public class GetDefaultUserAddressQueryHandler(
    IUserContext userContext,
    IUserRepository userRepository)
    : IRequestHandler<GetDefaultUserAddressQuery, UserAddressDto?>
{
    public async Task<UserAddressDto?> Handle(GetDefaultUserAddressQuery request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(userContext.UserId, includeDetail: true, cancellationToken)
            ?? throw new NotFoundException(UserDomainErrorCode.UserNotFound, nameof(User));

        var address = user.Addresses.FirstOrDefault(a => a.IsDefault);
        return address is null ? null : UserAddressDto.FromAddress(address);
    }
}
