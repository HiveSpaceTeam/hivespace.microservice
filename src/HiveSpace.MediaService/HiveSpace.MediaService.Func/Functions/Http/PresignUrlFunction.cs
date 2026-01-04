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

public class PresignUrlFunction(
    ILogger<PresignUrlFunction> logger,
    IMediaService mediaService,
    IValidator<PresignUrlRequest> validator)
{
    private readonly ILogger<PresignUrlFunction> _logger = logger;
    private readonly IMediaService _mediaService = mediaService;
    private readonly IValidator<PresignUrlRequest> _validator = validator;

    [Function("PresignUrl")]
    public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = ApiConfigs.PresignUrl)] HttpRequestData req)
    {
        _logger.LogInformation("Processing PresignUrl request.");

        var request = await req.ReadFromJsonAsync<PresignUrlRequest>() ?? throw ExceptionHelper.BadRequestException(CommonErrorCode.Required, "RequestBody");
        var validationResult = await _validator.ValidateAsync(request);
        ValidationHelper.ValidateResult(validationResult);

        var result = await _mediaService.GeneratePresignedUrlAsync(request);
        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(result);
        return response;
    }
}
