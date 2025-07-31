using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HiveSpace.Application.Shared.Handlers;
using HiveSpace.IdentityService.Domain.Repositories;
using HiveSpace.Core.Contexts;
using HiveSpace.IdentityService.Domain.Exceptions;

namespace HiveSpace.IdentityService.Application.Commands.Addresses;


public class SetDefaultAddressCommandHandler : ICommandHandler<SetDefaultAddressCommand>
{
    private readonly IUserRepository _userRepository;
    private readonly IUserContext _userContext;

    public SetDefaultAddressCommandHandler(IUserRepository userRepository, IUserContext userContext)
    {
        _userRepository = userRepository;
        _userContext = userContext;
    }

    public async Task Handle(SetDefaultAddressCommand command, CancellationToken cancellationToken)
    {
        var userId = _userContext.UserId.ToString();
        var user = await _userRepository.GetByIdAsync(userId, includeDetail: true)
            ?? throw new UserNotFoundException();

        if (!user.Addresses.Any(a => a.Id == command.AddressId))
            throw new AddressNotFoundException();

        user.SetDefaultAddress(command.AddressId);
        await _userRepository.SaveChangesAsync();
    }
}

