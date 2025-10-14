using System;
using System.Threading;
using System.Threading.Tasks;
using HiveSpace.Core.Contexts;
using HiveSpace.UserService.Application.Interfaces.Services;
using HiveSpace.UserService.Application.Models.Requests.Store;
using HiveSpace.UserService.Application.Models.Responses.Store;
using HiveSpace.UserService.Domain.Repositories;
using HiveSpace.UserService.Domain.Services;

namespace HiveSpace.UserService.Application.Services;

public class StoreService : IStoreService
{
    private readonly IUserContext _userContext;
    private readonly StoreManager _storeManager;
    private readonly IStoreRepository _storeRepository;
    
    public StoreService(
        IUserContext userContext,
        StoreManager storeManager,
        IStoreRepository storeRepository)
    {
        _userContext = userContext ?? throw new ArgumentNullException(nameof(userContext));
        _storeManager = storeManager ?? throw new ArgumentNullException(nameof(storeManager));
        _storeRepository = storeRepository ?? throw new ArgumentNullException(nameof(storeRepository));
    }
    
    public async Task<CreateStoreResponseDto> CreateStoreAsync(CreateStoreRequestDto request, CancellationToken cancellationToken = default)
    {
        var store = await _storeManager.RegisterStoreAsync(
            request.StoreName,
            request.Description,
            request.StoreLogoFileId,
            request.Address,
            _userContext.UserId,
            cancellationToken);

        // Save through repository
        _storeRepository.Add(store);
        
        await _storeRepository.SaveChangesAsync(cancellationToken);
        
        return new CreateStoreResponseDto(
            store.Id,
            store.StoreName,
            store.Description,
            store.LogoUrl,
            store.Address);
    }
}