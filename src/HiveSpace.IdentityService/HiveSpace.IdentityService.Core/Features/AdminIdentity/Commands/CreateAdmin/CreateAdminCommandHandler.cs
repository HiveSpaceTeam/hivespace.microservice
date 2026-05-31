using HiveSpace.Application.Shared.Handlers;
using HiveSpace.Core.Exceptions;
using HiveSpace.Core.Exceptions.Models;
using HiveSpace.IdentityService.Core.Exceptions;
using HiveSpace.IdentityService.Core.Features.AdminIdentity.Dtos;
using HiveSpace.IdentityService.Core.DomainModels;
using HiveSpace.IdentityService.Core.Interfaces.Messaging;
using HiveSpace.IdentityService.Core.Persistence;
using Microsoft.AspNetCore.Identity;

namespace HiveSpace.IdentityService.Core.Features.AdminIdentity.Commands.CreateAdmin;

public class CreateAdminCommandHandler(
    UserManager<ApplicationUser> userManager,
    IIdentityEventPublisher identityEventPublisher,
    IdentityDbContext dbContext)
    : ICommandHandler<CreateAdminCommand, CreateAdminResult>
{
    public async Task<CreateAdminResult> Handle(CreateAdminCommand command, CancellationToken cancellationToken)
    {
        if (command.Password != command.ConfirmPassword)
            throw new BadRequestException([new Error(IdentityDomainErrorCode.InvalidConfiguration, nameof(command.ConfirmPassword))]);

        var role = command.IsSystemAdmin ? "SystemAdmin" : "Admin";
        var user = new ApplicationUser
        {
            UserName       = command.Email.Trim(),
            Email          = command.Email.Trim(),
            RoleName       = role,
            Status         = UserStatus.Active,
            EmailConfirmed = true,
            CreatedAt      = DateTimeOffset.UtcNow,
            UpdatedAt      = DateTimeOffset.UtcNow
        };

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        var result = await userManager.CreateAsync(user, command.Password);
        if (!result.Succeeded)
            throw new BadRequestException(result.Errors.Select(e => new Error(IdentityDomainErrorCode.IdentityUserCreationFailed, e.Code)).ToArray());

        var roleResult = await userManager.AddToRoleAsync(user, role);
        if (!roleResult.Succeeded)
            throw new BadRequestException(roleResult.Errors.Select(e => new Error(IdentityDomainErrorCode.IdentityUserCreationFailed, e.Code)).ToArray());

        await identityEventPublisher.PublishIdentityUserCreatedAsync(user, command.FullName.Trim(), cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return AdminIdentityMapper.ToCreateAdminResult(user);
    }
}
