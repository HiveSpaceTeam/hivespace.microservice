using System.Text;
using HiveSpace.Application.Shared.Handlers;
using HiveSpace.Core.Exceptions;
using HiveSpace.Core.Exceptions.Models;
using HiveSpace.Domain.Shared.Enumerations;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.IdentityService.Core.Exceptions;
using HiveSpace.IdentityService.Core.Identity;
using HiveSpace.IdentityService.Core.Persistence;
using HiveSpace.Infrastructure.Messaging.Shared.Events.Users;
using MassTransit;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;

namespace HiveSpace.IdentityService.Core.Features.EmailVerification.Commands.ConfirmEmailVerification;

public class ConfirmEmailVerificationCommandHandler(
    UserManager<ApplicationUser> userManager,
    IPublishEndpoint publishEndpoint,
    IdentityDbContext dbContext)
    : ICommandHandler<ConfirmEmailVerificationCommand>
{
    public async Task Handle(ConfirmEmailVerificationCommand command, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(command.UserId)
            ?? throw new NotFoundException(IdentityDomainErrorCode.IdentityUserNotFound, nameof(command.UserId));

        if (await userManager.IsEmailConfirmedAsync(user))
            throw new ConflictException(IdentityDomainErrorCode.EmailAlreadyVerified, nameof(user.Email));

        string decodedToken;
        try
        {
            decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(command.Token));
        }
        catch
        {
            throw new BadRequestException([new Error(IdentityDomainErrorCode.EmailVerificationFailed, nameof(command.Token))]);
        }

        var result = await userManager.ConfirmEmailAsync(user, decodedToken);
        if (!result.Succeeded)
            throw new BadRequestException([new Error(IdentityDomainErrorCode.EmailVerificationFailed, nameof(command.Token))]);

        await publishEndpoint.Publish(new UserEmailVerifiedIntegrationEvent
        {
            UserId = user.Id,
            ToEmail = user.Email!,
            ToName = user.UserName ?? user.Email!,
            Locale = Culture.Vi
        }, cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
