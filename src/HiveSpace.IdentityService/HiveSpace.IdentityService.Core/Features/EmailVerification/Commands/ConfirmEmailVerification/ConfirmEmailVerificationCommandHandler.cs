using System.Text;
using HiveSpace.Application.Shared.Handlers;
using HiveSpace.Core.Exceptions;
using HiveSpace.Core.Exceptions.Models;
using HiveSpace.Domain.Shared.Enumerations;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.IdentityService.Core.Exceptions;
using HiveSpace.IdentityService.Core.DomainModels;
using HiveSpace.IdentityService.Core.Interfaces.Messaging;
using HiveSpace.IdentityService.Core.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;

namespace HiveSpace.IdentityService.Core.Features.EmailVerification.Commands.ConfirmEmailVerification;

public class ConfirmEmailVerificationCommandHandler(
    UserManager<ApplicationUser> userManager,
    IIdentityEventPublisher identityEventPublisher,
    IdentityDbContext dbContext)
    : ICommandHandler<ConfirmEmailVerificationCommand>
{
    public async Task Handle(ConfirmEmailVerificationCommand command, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(command.UserId)
            ?? throw new NotFoundException(IdentityDomainErrorCode.IdentityUserNotFound, nameof(command.UserId));

        var emailAlreadyConfirmed = await userManager.IsEmailConfirmedAsync(user);
        var readinessSideEffectsOwed = user.Status != UserStatus.Active || user.ActivatedAt is null;

        if (!emailAlreadyConfirmed)
        {
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

            readinessSideEffectsOwed = true;
        }

        if (!readinessSideEffectsOwed)
            return;

        user.Status = UserStatus.Active;
        user.ActivatedAt ??= DateTimeOffset.UtcNow;
        user.UpdatedAt = DateTimeOffset.UtcNow;

        await identityEventPublisher.PublishIdentityUserReadyAsync(
            user,
            user.FullName,
            cancellationToken);
        await identityEventPublisher.PublishEmailVerifiedAsync(user, Culture.Vi, cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
