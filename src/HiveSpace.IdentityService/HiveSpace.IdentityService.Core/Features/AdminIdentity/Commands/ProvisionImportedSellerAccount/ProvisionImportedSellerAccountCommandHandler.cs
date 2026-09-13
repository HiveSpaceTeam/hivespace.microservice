using HiveSpace.Application.Shared.Handlers;
using HiveSpace.Core.Exceptions;
using HiveSpace.Core.Exceptions.Models;
using HiveSpace.IdentityService.Core.DomainModels;
using HiveSpace.IdentityService.Core.Exceptions;
using HiveSpace.IdentityService.Core.Interfaces.Messaging;
using HiveSpace.IdentityService.Core.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace HiveSpace.IdentityService.Core.Features.AdminIdentity.Commands.ProvisionImportedSellerAccount;

public class ProvisionImportedSellerAccountCommandHandler(
    UserManager<ApplicationUser> userManager,
    IdentityDbContext dbContext,
    IIdentityEventPublisher identityEventPublisher)
    : ICommandHandler<ProvisionImportedSellerAccountCommand, ProvisionImportedSellerAccountResult>
{
    private const string SellerRole = "Seller";

    public async Task<ProvisionImportedSellerAccountResult> Handle(
        ProvisionImportedSellerAccountCommand command,
        CancellationToken cancellationToken)
    {
        var identityKey = ImportedSellerIdentityKey.Create(command.SourceSystem, command.ExternalSellerId);
        var existing = await dbContext.Users
            .FirstOrDefaultAsync(u => u.Email == identityKey.Email || u.UserName == identityKey.UserName, cancellationToken);

        if (existing is not null)
        {
            var exactExternalIdentity = string.Equals(existing.Email, identityKey.Email, StringComparison.OrdinalIgnoreCase)
                && string.Equals(existing.UserName, identityKey.UserName, StringComparison.OrdinalIgnoreCase);
            if (!exactExternalIdentity)
                return new ProvisionImportedSellerAccountResult(existing.Id, "Conflict", "ImportedSellerIdentityConflict");

            return new ProvisionImportedSellerAccountResult(existing.Id, "Matched", null);
        }

        var now = DateTimeOffset.UtcNow;
        var user = new ApplicationUser
        {
            UserName = identityKey.UserName,
            NormalizedUserName = identityKey.UserName.ToUpperInvariant(),
            Email = identityKey.Email,
            NormalizedEmail = identityKey.Email.ToUpperInvariant(),
            FullName = command.DisplayName.Trim(),
            RoleName = SellerRole,
            Status = UserStatus.Active,
            EmailConfirmed = true,
            CreatedAt = now,
            UpdatedAt = now,
            ActivatedAt = now
        };

        var createResult = await userManager.CreateAsync(user, GeneratePassword(command.SourceSystem, command.ExternalSellerId));
        if (!createResult.Succeeded)
            throw ToBadRequest(createResult);

        var roleResult = await userManager.AddToRoleAsync(user, SellerRole);
        if (!roleResult.Succeeded)
            throw ToBadRequest(roleResult);

        await identityEventPublisher.PublishIdentityUserReadyAsync(user, user.FullName, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new ProvisionImportedSellerAccountResult(user.Id, "Created", null);
    }

    private static BadRequestException ToBadRequest(IdentityResult result)
        => new(result.Errors.Select(e => new Error(IdentityDomainErrorCode.IdentityUserCreationFailed, e.Code)).ToArray());

    private static string GeneratePassword(string sourceSystem, string externalSellerId)
    {
        var normalizedSource = NormalizePasswordSegment(sourceSystem);
        var normalizedExternalSellerId = NormalizePasswordSegment(externalSellerId);
        return $"Seller_{normalizedSource}_{normalizedExternalSellerId}_123$";
    }

    private static string NormalizePasswordSegment(string value)
        => string.IsNullOrWhiteSpace(value)
            ? "unknown"
            : Regex.Replace(value.Trim().ToLowerInvariant(), "[^a-z0-9]+", "-").Trim('-') switch
            {
                "" => "unknown",
                var normalized => normalized
            };
}
