using HiveSpace.Core;
using HiveSpace.Core.Helpers;
using HiveSpace.UserService.Application.Interfaces.Services;
using HiveSpace.UserService.Application.Models.Requests.Store;
using HiveSpace.UserService.Application.Models.Responses.Store;
using HiveSpace.UserService.Application.Validators.Store;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HiveSpace.UserService.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/stores")]
[ApiVersion("1.0")]
public class StoreController : ControllerBase
{
    private readonly IStoreService _storeService;
    
    public StoreController(IStoreService storeService)
    {
        _storeService = storeService;
    }
    
    [HttpPost]
    [Authorize(Policy = "RequireUserFullAccessScope")]
    public async Task<ActionResult<CreateStoreResponseDto>> CreateStore(
        [FromBody] CreateStoreRequestDto request,
        CancellationToken cancellationToken)
    {
        ValidationHelper.ValidateResult(new CreateStoreValidator().Validate(request));
        
        var result = await _storeService.CreateStoreAsync(request, cancellationToken);
        return CreatedAtAction(nameof(CreateStore), new { id = result.StoreId }, result);
    }
}