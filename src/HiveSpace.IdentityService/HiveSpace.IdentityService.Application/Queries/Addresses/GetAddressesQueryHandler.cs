using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HiveSpace.Application.Shared.Handlers;
using HiveSpace.IdentityService.Application.Models.Responses;
using HiveSpace.IdentityService.Domain.Repositories;
using HiveSpace.Core.Contexts;
using HiveSpace.IdentityService.Domain.Exceptions;

namespace HiveSpace.IdentityService.Application.Queries.Addresses;

public class GetAddressesQueryHandler : IQueryHandler<GetAddressesQuery, IEnumerable<AddressResponseDto>>
{
    private readonly IUserRepository _userRepository;
    private readonly IUserContext _userContext;

    public GetAddressesQueryHandler(IUserRepository userRepository, IUserContext userContext)
    {
        _userRepository = userRepository;
        _userContext = userContext;
    }

    public async Task<IEnumerable<AddressResponseDto>> Handle(GetAddressesQuery query, CancellationToken cancellationToken)
    {
        var userId = _userContext.UserId.ToString();
        var user = await _userRepository.GetByIdAsync(userId, includeDetail: true)
            ?? throw new UserNotFoundException();

        var addresses = user.Addresses
            .OrderByDescending(a => a.IsDefault)
            .ThenBy(a => a.CreatedAt);

        return addresses.Select(address => new AddressResponseDto(
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
        ));
    }
}

