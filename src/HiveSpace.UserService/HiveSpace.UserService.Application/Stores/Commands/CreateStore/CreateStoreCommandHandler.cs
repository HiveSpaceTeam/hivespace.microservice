using HiveSpace.Application.Shared.Handlers;
using HiveSpace.Core.Contexts;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.UserService.Application.Interfaces.Messaging;
using HiveSpace.UserService.Application.Stores.Dtos;
using HiveSpace.UserService.Application.Stores.IdentityProvisioning;
using HiveSpace.UserService.Domain.Exceptions;
using HiveSpace.UserService.Domain.Repositories;
using HiveSpace.UserService.Domain.Services;
using HiveSpace.Infrastructure.Persistence.Transaction;

namespace HiveSpace.UserService.Application.Stores.Commands.CreateStore;

public class CreateStoreCommandHandler(
    IUserContext userContext,
    StoreManager storeManager,
    IStoreRepository storeRepository,
    IIdentitySellerProvisioningClient identitySellerProvisioningClient,
    IStoreEventPublisher storeEventPublisher,
    ITransactionService transactionService)
    : ICommandHandler<CreateStoreCommand, CreateStoreResponseDto>
{
    public async Task<CreateStoreResponseDto> Handle(CreateStoreCommand request, CancellationToken cancellationToken)
    {
        CreateStoreResponseDto? response = null;

        await transactionService.InTransactionScopeAsync(async _ =>
        {
            var payload = request.Payload;
            var registration = await storeManager.RegisterStoreAsync(
                payload.StoreName,
                payload.Description,
                payload.StoreLogoFileId,
                payload.Address,
                userContext.UserId,
                Guid.NewGuid(),
                cancellationToken);

            storeRepository.Add(registration.Store);
            await storeRepository.SaveChangesAsync(cancellationToken);

            var provisioning = await identitySellerProvisioningClient.ProvisionSellerIdentityAsync(
                userContext.UserId,
                registration.Store.Id,
                registration.Store.StoreName,
                CreateCorrelationId(),
                cancellationToken);

            if (!provisioning.Succeeded)
            {
                throw new ConflictException(
                    UserDomainErrorCode.SellerIdentityProvisioningFailed,
                    nameof(IdentitySellerProvisioningResult));
            }

            await storeEventPublisher.PublishStoreCreatedAsync(registration.Store, cancellationToken);

            response = new CreateStoreResponseDto(
                registration.Store.Id,
                registration.Store.StoreName,
                registration.Store.Description,
                registration.Store.LogoFileId,
                registration.Store.Address);
        }, actionName: nameof(CreateStoreCommand));

        return response!;
    }

    private static string CreateCorrelationId()
        => System.Diagnostics.Activity.Current?.Id ?? Guid.NewGuid().ToString("N");
}
