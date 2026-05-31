using HiveSpace.Application.Shared.Handlers;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.IdentityService.Core.DomainModels;
using HiveSpace.IdentityService.Core.Exceptions;
using HiveSpace.IdentityService.Core.Features.AdminIdentity.Dtos;
using HiveSpace.IdentityService.Core.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HiveSpace.IdentityService.Core.Features.AdminIdentity.Commands.DeleteUser;

public class DeleteUserCommandHandler(IdentityDbContext dbContext)
    : ICommandHandler<DeleteUserCommand, DeleteIdentityUserResult>
{
    public async Task<DeleteIdentityUserResult> Handle(DeleteUserCommand command, CancellationToken cancellationToken)
    {
        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == command.UserId, cancellationToken)
            ?? throw new NotFoundException(IdentityDomainErrorCode.IdentityUserNotFound, nameof(command.UserId));

        user.Status = UserStatus.Inactive;
        user.UpdatedAt = DateTimeOffset.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);

        return new DeleteIdentityUserResult(
            user.Id,
            user.UserName ?? string.Empty,
            AdminIdentityMapper.GetDisplayName(user),
            user.Email ?? string.Empty,
            (int)user.Status,
            user.CreatedAt,
            user.UpdatedAt,
            user.LastLoginAt,
            user.UpdatedAt,
            null,
            string.Equals(user.RoleName, "Seller", StringComparison.OrdinalIgnoreCase),
            command.DeletedBy);
    }
}
