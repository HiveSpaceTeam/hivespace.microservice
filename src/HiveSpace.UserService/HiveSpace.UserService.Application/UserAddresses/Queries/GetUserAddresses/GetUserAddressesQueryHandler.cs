using HiveSpace.Application.Shared.Handlers;
using HiveSpace.Core.Contexts;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.UserService.Application.UserAddresses.Dtos;
using HiveSpace.UserService.Domain.Aggregates.User;
using HiveSpace.UserService.Domain.Exceptions;
using HiveSpace.UserService.Domain.Repositories;

namespace HiveSpace.UserService.Application.UserAddresses.Queries.GetUserAddresses;

public class GetUserAddressesQueryHandler(
    IUserContext userContext,
    IUserRepository userRepository)
    : IQueryHandler<GetUserAddressesQuery, List<UserAddressDto>>
{
    public async Task<List<UserAddressDto>> Handle(GetUserAddressesQuery request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(userContext.UserId, includeDetail: true, cancellationToken)
            ?? throw new NotFoundException(UserDomainErrorCode.UserNotFound, nameof(User));

        return user.Addresses.Select(UserAddressDto.FromAddress).ToList();
    }
}
