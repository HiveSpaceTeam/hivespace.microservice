using HiveSpace.Application.Shared.Handlers;
using HiveSpace.Core.Exceptions;
using HiveSpace.Core.Exceptions.Models;
using HiveSpace.IdentityService.Core.DomainModels;
using HiveSpace.IdentityService.Core.Exceptions;
using HiveSpace.IdentityService.Core.Features.AccountSessions.Commands;
using HiveSpace.IdentityService.Core.Features.AccountSessions.Dtos;
using HiveSpace.IdentityService.Core.Interfaces.Messaging;
using HiveSpace.IdentityService.Core.Interfaces.Services;
using HiveSpace.IdentityService.Core.Persistence;
using Microsoft.AspNetCore.Identity;
using ConflictException = HiveSpace.Domain.Shared.Exceptions.ConflictException;

namespace HiveSpace.IdentityService.Core.Features.AccountSessions.Commands.RegisterAccount;

public class RegisterAccountCommandHandler(
    UserManager<ApplicationUser> userManager,
    IIdentityEventPublisher identityEventPublisher,
    IdentityDbContext dbContext,
    IAccountSessionIssuer accountSessionIssuer)
    : ICommandHandler<RegisterAccountCommand, SessionResponse>
{
    public async Task<SessionResponse> Handle(RegisterAccountCommand command, CancellationToken cancellationToken)
    {
        if (AccountSessionHandlerBase.NormalizeApp(command.App) == "admin")
            throw new ForbiddenException([new Error(IdentityDomainErrorCode.AccountNotAllowed, nameof(command.App))]);

        var email = command.Email.Trim();
        var existingUser = await userManager.FindByEmailAsync(email);
        if (existingUser is not null)
            throw new ConflictException(IdentityDomainErrorCode.DuplicateEmail, nameof(command.Email));

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            RoleName = "Buyer",
            Status = UserStatus.Active,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        var result = await userManager.CreateAsync(user, command.Password);
        if (!result.Succeeded)
            throw new BadRequestException(result.Errors.Select(e => new Error(IdentityDomainErrorCode.IdentityUserCreationFailed, e.Code)));

        await identityEventPublisher.PublishIdentityUserCreatedAsync(user, command.FullName, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return await accountSessionIssuer.IssueAsync(
            user,
            command.App,
            command.ReturnUrl,
            updateLastLogin: false,
            cancellationToken);
    }
}
