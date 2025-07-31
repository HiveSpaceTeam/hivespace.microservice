using HiveSpace.Application.Shared.Handlers;
using HiveSpace.IdentityService.Domain.Repositories;
using HiveSpace.Core.Contexts;
using HiveSpace.IdentityService.Domain.Exceptions;

namespace HiveSpace.IdentityService.Application.Commands.Addresses;

public class DeleteAddressCommandHandler : ICommandHandler<DeleteAddressCommand>
{
    private readonly IUserRepository _userRepository;
    private readonly IUserContext _userContext;

    public DeleteAddressCommandHandler(IUserRepository userRepository, IUserContext userContext)
    {
        _userRepository = userRepository;
        _userContext = userContext;
    }

    public async Task Handle(DeleteAddressCommand command, CancellationToken cancellationToken)
    {
        var userId = _userContext.UserId.ToString();
        var user = await _userRepository.GetByIdAsync(userId, includeDetail: true)
            ?? throw new UserNotFoundException();

        if (!user.Addresses.Any(a => a.Id == command.AddressId))
            throw new AddressNotFoundException();

        user.RemoveAddress(command.AddressId);
        await _userRepository.SaveChangesAsync();
    }
}

