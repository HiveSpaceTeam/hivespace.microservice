using HiveSpace.Application.Shared.Handlers;
using HiveSpace.Core.Exceptions;
using HiveSpace.Core.Exceptions.Models;
using HiveSpace.IdentityService.Core.Exceptions;
using HiveSpace.IdentityService.Core.Features.AdminIdentity.Dtos;
using HiveSpace.IdentityService.Core.Identity;
using HiveSpace.IdentityService.Core.Persistence;
using HiveSpace.Infrastructure.Messaging.Shared.Events.Users;
using MassTransit;
using Microsoft.AspNetCore.Identity;

namespace HiveSpace.IdentityService.Core.Features.AdminIdentity.Commands.CreateAdmin;

public class CreateAdminCommandHandler(
    UserManager<ApplicationUser> userManager,
    IPublishEndpoint publishEndpoint,
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
            Status         = 1,
            EmailConfirmed = true,
            CreatedAt      = DateTimeOffset.UtcNow,
            UpdatedAt      = DateTimeOffset.UtcNow
        };

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        var result = await userManager.CreateAsync(user, command.Password);
        if (!result.Succeeded)
            throw new BadRequestException(result.Errors.Select(e => new Error(IdentityDomainErrorCode.IdentityUserCreationFailed, e.Code)).ToArray());

        await publishEndpoint.Publish(new IdentityUserCreatedIntegrationEvent
        {
            UserId        = user.Id,
            Email         = user.Email!,
            UserName      = user.UserName,
            FullName      = command.FullName.Trim(),
            OccurredAt    = DateTime.UtcNow,
            CorrelationId = Guid.NewGuid()
        }, cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return AdminIdentityMapper.ToCreateAdminResult(user);
    }
}
