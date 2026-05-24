using HiveSpace.Application.Shared.Handlers;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.IdentityService.Core.Exceptions;
using HiveSpace.IdentityService.Core.Identity;
using HiveSpace.IdentityService.Core.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HiveSpace.IdentityService.Core.Features.Roles.Commands.AssignSellerRole;

public class AssignSellerRoleCommandHandler(IdentityDbContext dbContext, UserManager<ApplicationUser> userManager)
    : ICommandHandler<AssignSellerRoleCommand>
{
    public async Task Handle(AssignSellerRoleCommand command, CancellationToken cancellationToken)
    {
        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == command.UserId, cancellationToken)
            ?? throw new NotFoundException(IdentityDomainErrorCode.IdentityUserNotFound, nameof(command.UserId));

        const string sellerRole = "Seller";
        if (string.Equals(user.RoleName, sellerRole, StringComparison.OrdinalIgnoreCase) && user.StoreId == command.StoreId)
            return;

        user.RoleName = sellerRole;
        user.StoreId = command.StoreId;
        user.UpdatedAt = DateTimeOffset.UtcNow;

        if (!await userManager.IsInRoleAsync(user, sellerRole))
        {
            var result = await userManager.AddToRoleAsync(user, sellerRole);
            if (!result.Succeeded)
                throw new ConflictException(IdentityDomainErrorCode.InvalidConfiguration, nameof(sellerRole));
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
