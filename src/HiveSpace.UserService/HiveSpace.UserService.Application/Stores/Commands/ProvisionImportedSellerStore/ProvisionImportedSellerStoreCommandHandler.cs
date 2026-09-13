using HiveSpace.Application.Shared.Handlers;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.UserService.Application.Interfaces.Messaging;
using HiveSpace.UserService.Domain.Aggregates.User;
using HiveSpace.UserService.Domain.Exceptions;
using HiveSpace.UserService.Domain.Repositories;
using HiveSpace.UserService.Domain.Services;
using System.Globalization;
using System.Text;

namespace HiveSpace.UserService.Application.Stores.Commands.ProvisionImportedSellerStore;

public class ProvisionImportedSellerStoreCommandHandler(
    IUserRepository userRepository,
    IStoreRepository storeRepository,
    StoreManager storeManager,
    IStoreEventPublisher storeEventPublisher,
    IUserEventPublisher userEventPublisher)
    : ICommandHandler<ProvisionImportedSellerStoreCommand, ProvisionImportedSellerStoreResult>
{
    private const string PlaceholderLogoFileId = "imported-seller-placeholder-logo";

    public async Task<ProvisionImportedSellerStoreResult> Handle(
        ProvisionImportedSellerStoreCommand command,
        CancellationToken cancellationToken)
    {
        var existingStore = await storeRepository.GetByOwnerIdAsync(command.UserId, cancellationToken);
        if (existingStore is not null)
            return new ProvisionImportedSellerStoreResult(existingStore.Id, "Matched", null);

        if (await HasSimilarStoreNameOwnedByAnotherUserAsync(command.StoreName, command.UserId, cancellationToken))
            return new ProvisionImportedSellerStoreResult(null, "Conflict", "SimilarStoreNameConflict");

        await EnsureProfileAsync(command, cancellationToken);

        try
        {
            var registration = await storeManager.RegisterStoreAsync(
                command.StoreName,
                $"Imported seller store from {command.SourceSystem}. Source: {command.SourceUrl ?? command.ExternalSellerId}",
                PlaceholderLogoFileId,
                $"Imported seller address pending review ({command.SourceSystem}:{command.ExternalSellerId})",
                command.UserId,
                null,
                cancellationToken);

            var logoUrl = Normalize(command.LogoUrl);
            if (logoUrl is not null)
                registration.Store.SetLogoUrl(logoUrl);

            storeRepository.Add(registration.Store);
            await storeEventPublisher.PublishStoreCreatedAsync(registration.Store, cancellationToken);
            await storeRepository.SaveChangesAsync(cancellationToken);

            return new ProvisionImportedSellerStoreResult(registration.Store.Id, "Created", null);
        }
        catch (ConflictException ex) when (ex.ErrorCode == UserDomainErrorCode.StoreNameAlreadyExists)
        {
            return new ProvisionImportedSellerStoreResult(null, "Conflict", "StoreNameAlreadyExists");
        }
        catch (UserStoreExistsException)
        {
            var matchedStore = await storeRepository.GetByOwnerIdAsync(command.UserId, cancellationToken);
            return matchedStore is null
                ? new ProvisionImportedSellerStoreResult(null, "Conflict", "UserStoreAlreadyExists")
                : new ProvisionImportedSellerStoreResult(matchedStore.Id, "Matched", null);
        }
    }

    private async Task EnsureProfileAsync(ProvisionImportedSellerStoreCommand command, CancellationToken cancellationToken)
    {
        var existing = await userRepository.GetByIdAsync(command.UserId, cancellationToken: cancellationToken);
        if (existing is not null)
            return;

        var key = ImportedSellerProfileKey.Create(command.SourceSystem, command.ExternalSellerId);
        var profile = User.CreateProfile(
            command.UserId,
            Email.Create(key.Email),
            key.UserName,
            command.StoreName.Trim());

        await userRepository.CreateUserAsync(profile, string.Empty, cancellationToken);
        await userEventPublisher.PublishUserCreatedAsync(profile, cancellationToken);
    }

    private async Task<bool> HasSimilarStoreNameOwnedByAnotherUserAsync(
        string storeName,
        Guid ownerId,
        CancellationToken cancellationToken)
    {
        var candidates = await storeRepository.ListStoreNameCandidatesAsync(cancellationToken);
        return candidates.Any(store => store.OwnerId != ownerId && IsSimilarName(store.StoreName, storeName));
    }

    private static bool IsSimilarName(string existing, string incoming)
    {
        var normalizedExisting = NormalizeName(existing);
        var normalizedIncoming = NormalizeName(incoming);
        if (string.IsNullOrWhiteSpace(normalizedExisting) || string.IsNullOrWhiteSpace(normalizedIncoming))
            return false;

        if (string.Equals(normalizedExisting, normalizedIncoming, StringComparison.OrdinalIgnoreCase))
            return false;

        var existingTokens = normalizedExisting.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var incomingTokens = normalizedIncoming.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (existingTokens.Length == 0 || incomingTokens.Length == 0)
            return false;

        var shorter = existingTokens.Length <= incomingTokens.Length ? existingTokens : incomingTokens;
        var longer = existingTokens.Length > incomingTokens.Length ? existingTokens : incomingTokens;

        if (!IsOrderedPrefix(shorter, longer))
            return false;

        return shorter.Any(token => token.Length >= 7)
               || shorter.Any(token =>
                   string.Equals(token, "official", StringComparison.OrdinalIgnoreCase)
                   || string.Equals(token, "store", StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsOrderedPrefix(string[] shorter, string[] longer)
    {
        if (shorter.Length >= longer.Length)
            return false;

        for (var index = 0; index < shorter.Length; index++)
        {
            if (!string.Equals(shorter[index], longer[index], StringComparison.OrdinalIgnoreCase))
                return false;
        }

        return true;
    }

    private static string NormalizeName(string value)
    {
        var normalized = value.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(normalized.Length);
        var previousWasSpace = false;

        foreach (var ch in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(ch) == UnicodeCategory.NonSpacingMark)
                continue;

            if (char.IsLetterOrDigit(ch))
            {
                builder.Append(ch);
                previousWasSpace = false;
                continue;
            }

            if (!previousWasSpace)
            {
                builder.Append(' ');
                previousWasSpace = true;
            }
        }

        return builder.ToString().Trim().Normalize(NormalizationForm.FormC);
    }

    private static string? Normalize(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
