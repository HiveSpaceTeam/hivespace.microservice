using System.Net;
using FluentValidation;
using HiveSpace.Core.Exceptions;
using HiveSpace.Core.Helpers;
using HiveSpace.MediaService.Func.Core.Constants;
using HiveSpace.MediaService.Func.Core.Contracts;
using HiveSpace.MediaService.Func.Core.Interfaces;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace HiveSpace.MediaService.Func.Functions.Http;

public class ConfirmUploadFunction(
    ILogger<ConfirmUploadFunction> logger
    , IMediaService mediaService,
    IValidator<ConfirmUploadRequest> validator
    )
{
    private readonly ILogger<ConfirmUploadFunction> _logger = logger;
    private readonly IMediaService _mediaService = mediaService;
    private readonly IValidator<ConfirmUploadRequest> _validator = validator;

    [Function("ConfirmUpload")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = ApiConfigs.ConfirmUpload)] HttpRequestData req,
        string fileId)
    {
        _logger.LogInformation("Processing ConfirmUpload request for FileID: {FileId}", fileId);

        var requestBody = await req.ReadFromJsonAsync<ConfirmUploadRequest>() ?? throw ExceptionHelper.BadRequestException(CommonErrorCode.Required, "RequestBody");
        var request = requestBody with { Id = Guid.Parse(fileId) };
        var validationResult = await _validator.ValidateAsync(request);
        ValidationHelper.ValidateResult(validationResult);

        await _mediaService.ConfirmUploadAsync(request);
        var response = req.CreateResponse(HttpStatusCode.NoContent);
        return response;
    }
}
