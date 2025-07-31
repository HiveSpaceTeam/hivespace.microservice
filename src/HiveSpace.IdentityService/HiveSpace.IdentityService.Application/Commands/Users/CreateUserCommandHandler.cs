
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using HiveSpace.Application.Shared.Handlers;
using HiveSpace.IdentityService.Application.Models.Responses;
using HiveSpace.IdentityService.Domain.Aggregates;
using HiveSpace.IdentityService.Domain.Repositories;
using HiveSpace.IdentityService.Domain.Exceptions;
using HiveSpace.Core.Contexts;
using HiveSpace.Infrastructure.Persistence.Transaction;
using HiveSpace.IdentityService.Application.Constants;

namespace HiveSpace.IdentityService.Application.Commands.Users;

public class CreateUserCommandHandler : ICommandHandler<CreateUserCommand, SignupResponseDto>
{
    private readonly IUserRepository _userRepository;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUserContext _userContext;
    private readonly ITransactionService _transactionService;

    public CreateUserCommandHandler(
        IUserRepository userRepository,
        UserManager<ApplicationUser> userManager,
        IUserContext userContext,
        ITransactionService transactionService)
    {
        _userRepository = userRepository;
        _userManager = userManager;
        _userContext = userContext;
        _transactionService = transactionService;
    }

    public async Task<SignupResponseDto> Handle(CreateUserCommand command, CancellationToken cancellationToken)
    {
        SignupResponseDto resultDto = default!;
        await _transactionService.InTransactionScopeAsync(async _ =>
        {
            if (await _userRepository.IsEmailExistsAsync(command.Email))
                throw new EmailAlreadyExistsException();
            if (await _userRepository.IsUserNameExistsAsync(command.UserName))
                throw new UserAlreadyExistsException();

            var user = new ApplicationUser(command.Email, command.UserName, command.FullName, null, null, null);
            var result = await _userManager.CreateAsync(user, command.Password);
            HandlePasswordErrors(result);

            await _userRepository.SaveChangesAsync();

            resultDto = new SignupResponseDto(
                user.Email ?? string.Empty,
                user.FullName ?? string.Empty,
                user.UserName ?? string.Empty,
                Guid.Parse(user.Id)
            );
        }, true, nameof(CreateUserCommandHandler));
        return resultDto;
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

