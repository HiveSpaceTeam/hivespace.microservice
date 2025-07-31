using Microsoft.AspNetCore.Identity;
using HiveSpace.Application.Shared.Handlers;
using HiveSpace.IdentityService.Domain.Aggregates;
using HiveSpace.IdentityService.Domain.Repositories;
using HiveSpace.IdentityService.Domain.Exceptions;
using HiveSpace.Core.Contexts;
using HiveSpace.IdentityService.Application.Constants;

namespace HiveSpace.IdentityService.Application.Commands.Users;

public class ChangePasswordCommandHandler : ICommandHandler<ChangePasswordCommand>
{
    private readonly IUserRepository _userRepository;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUserContext _userContext;

    public ChangePasswordCommandHandler(IUserRepository userRepository, UserManager<ApplicationUser> userManager, IUserContext userContext)
    {
        _userRepository = userRepository;
        _userManager = userManager;
        _userContext = userContext;
    }

    public async Task Handle(ChangePasswordCommand command, CancellationToken cancellationToken)
    {
        var userId = _userContext.UserId.ToString();
        var user = await _userRepository.GetByIdAsync(userId) ?? throw new UserNotFoundException();
        var result = await _userManager.ChangePasswordAsync(user, command.Password, command.NewPassword);
        HandlePasswordErrors(result);
    }

    private static void HandlePasswordErrors(IdentityResult result)
    {
        if (result.Succeeded) return;
        if (result.Errors.Any(e =>
            e.Code == IdentityResultError.PasswordTooShort ||
            e.Code == IdentityResultError.PasswordRequiresNonAlphanumeric ||
            e.Code == IdentityResultError.PasswordRequiresDigit ||
            e.Code == IdentityResultError.PasswordRequiresLower ||
            e.Code == IdentityResultError.PasswordRequiresUpper))
        {
            throw new PasswordTooWeakException();
        }
        throw new InvalidPasswordException();
    }
}

