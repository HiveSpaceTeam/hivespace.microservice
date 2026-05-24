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

namespace HiveSpace.IdentityService.Core.Features.EmailVerification.Commands.SendEmailVerification;

public class SendEmailVerificationCommandHandler(
    UserManager<ApplicationUser> userManager,
    IPublishEndpoint publishEndpoint,
    IdentityDbContext dbContext)
    : ICommandHandler<SendEmailVerificationCommand>
{
    public async Task Handle(SendEmailVerificationCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.CallbackUrl))
            throw new BadRequestException([new Error(IdentityDomainErrorCode.InvalidConfiguration, nameof(command.CallbackUrl))]);

        var user = await userManager.FindByIdAsync(command.UserId.ToString())
            ?? throw new NotFoundException(IdentityDomainErrorCode.IdentityUserNotFound, nameof(command.UserId));

        if (await userManager.IsEmailConfirmedAsync(user))
            throw new ConflictException(IdentityDomainErrorCode.EmailAlreadyVerified, nameof(user.Email));

        var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
        var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

        var verificationLink = $"{command.CallbackUrl}?userId={Uri.EscapeDataString(command.UserId.ToString())}&token={encodedToken}";
        if (!string.IsNullOrWhiteSpace(command.ReturnUrl))
            verificationLink += $"&returnUrl={Uri.EscapeDataString(command.ReturnUrl)}";

        await publishEndpoint.Publish(new UserEmailVerificationRequestedIntegrationEvent
        {
            UserId = command.UserId,
            ToEmail = user.Email!,
            ToName = user.UserName ?? user.Email!,
            VerificationLink = verificationLink,
            ExpiresAt = DateTime.UtcNow.AddHours(24),
            Locale = Culture.Vi
        }, cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
