using HiveSpace.Application.Shared.Handlers;
using HiveSpace.IdentityService.Core.DomainModels;
using HiveSpace.IdentityService.Core.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HiveSpace.IdentityService.Core.Features.Roles.Commands.ProvisionSellerIdentity;

public class ProvisionSellerIdentityCommandHandler(
    IdentityDbContext dbContext,
    UserManager<ApplicationUser> userManager)
    : ICommandHandler<ProvisionSellerIdentityCommand, ProvisionSellerIdentityResult>
{
    private const string SellerRole = "Seller";

    public async Task<ProvisionSellerIdentityResult> Handle(
        ProvisionSellerIdentityCommand command,
        CancellationToken cancellationToken)
    {
        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == command.UserId, cancellationToken);
        if (user is null)
        {
            return Failed(command, "IdentityUserNotFound", "Identity user was not found.");
        }

        if (string.Equals(user.RoleName, SellerRole, StringComparison.OrdinalIgnoreCase)
            && user.StoreId == command.StoreId)
        {
            return new ProvisionSellerIdentityResult(
                ProvisionSellerIdentityOutcome.Matched,
                user.Id,
                command.StoreId,
                null,
                null);
        }

        if (string.Equals(user.RoleName, SellerRole, StringComparison.OrdinalIgnoreCase)
            && user.StoreId is not null
            && user.StoreId != command.StoreId)
        {
            return Conflict(command, user.Id, "SellerStoreConflict", "Seller identity is already linked to another store.");
        }

        user.RoleName = SellerRole;
        user.StoreId = command.StoreId;
        user.UpdatedAt = DateTimeOffset.UtcNow;

        if (!await userManager.IsInRoleAsync(user, SellerRole))
        {
            var result = await userManager.AddToRoleAsync(user, SellerRole);
            if (!result.Succeeded)
                return Failed(command, "SellerRoleAssignmentFailed", "Seller role assignment failed.");
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return new ProvisionSellerIdentityResult(
            ProvisionSellerIdentityOutcome.Created,
            user.Id,
            command.StoreId,
            null,
            null);
    }

    private static ProvisionSellerIdentityResult Conflict(
        ProvisionSellerIdentityCommand command,
        Guid identityUserId,
        string failureCode,
        string failureMessage)
        => new(
            ProvisionSellerIdentityOutcome.Conflict,
            identityUserId,
            command.StoreId,
            failureCode,
            failureMessage);

    private static ProvisionSellerIdentityResult Failed(
        ProvisionSellerIdentityCommand command,
        string failureCode,
        string failureMessage)
        => new(
            ProvisionSellerIdentityOutcome.Failed,
            null,
            command.StoreId,
            failureCode,
            failureMessage);
}
