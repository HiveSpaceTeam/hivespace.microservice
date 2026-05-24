using HiveSpace.Application.Shared.Handlers;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.IdentityService.Core.Exceptions;
using HiveSpace.IdentityService.Core.Features.AdminIdentity.Dtos;
using HiveSpace.IdentityService.Core.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HiveSpace.IdentityService.Core.Features.AdminIdentity.Commands.SetUserStatus;

public class SetUserStatusCommandHandler(IdentityDbContext dbContext)
    : ICommandHandler<SetUserStatusCommand, SetIdentityStatusResult>
{
    public async Task<SetIdentityStatusResult> Handle(SetUserStatusCommand command, CancellationToken cancellationToken)
    {
        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == command.UserId, cancellationToken)
            ?? throw new NotFoundException(IdentityDomainErrorCode.IdentityUserNotFound, nameof(command.UserId));

        user.Status = command.IsActive ? 1 : 0;
        user.UpdatedAt = DateTimeOffset.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);

        return AdminIdentityMapper.ToStatusResult(user);
    }
}
