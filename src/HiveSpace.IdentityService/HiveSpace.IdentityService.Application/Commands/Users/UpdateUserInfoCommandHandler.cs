using System.Threading.Tasks;
using HiveSpace.Application.Shared.Handlers;
using HiveSpace.IdentityService.Domain.Aggregates;
using HiveSpace.IdentityService.Domain.Repositories;
using HiveSpace.IdentityService.Domain.Exceptions;
using HiveSpace.Core.Contexts;

namespace HiveSpace.IdentityService.Application.Commands.Users;

public class UpdateUserInfoCommandHandler : ICommandHandler<UpdateUserInfoCommand>
{
    private readonly IUserRepository _userRepository;
    private readonly IUserContext _userContext;

    public UpdateUserInfoCommandHandler(IUserRepository userRepository, IUserContext userContext)
    {
        _userRepository = userRepository;
        _userContext = userContext;
    }

    public async Task Handle(UpdateUserInfoCommand command, CancellationToken cancellationToken)
    {
        var userId = _userContext.UserId.ToString();
        var user = await _userRepository.GetByIdAsync(userId) ?? throw new UserNotFoundException();
        user.UpdateUserInfo(
            command.UserName,
            command.FullName,
            command.Email,
            command.PhoneNumber,
            command.Gender,
            command.DateOfBirth
        );
        await _userRepository.SaveChangesAsync();
    }
}

