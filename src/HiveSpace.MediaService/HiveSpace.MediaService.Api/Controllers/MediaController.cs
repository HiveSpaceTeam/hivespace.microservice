using HiveSpace.MediaService.Core.Contracts;
using HiveSpace.MediaService.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HiveSpace.MediaService.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/media")]
public class MediaController(IMediaService mediaService) : ControllerBase
{
    /// <summary>
    /// Generates a presigned URL for uploading a media asset.
    /// </summary>
    [HttpPost("presign-url")]
    [ProducesResponseType(typeof(PresignUrlResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> PresignUrl([FromBody] PresignUrlRequest request, CancellationToken cancellationToken)
    {
        var result = await mediaService.GeneratePresignedUrlAsync(request);
        return Ok(result);
    }

    /// <summary>
    /// Confirms that a media asset has been uploaded successfully.
    /// </summary>
    [HttpPost("{fileId:guid}/confirm")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ConfirmUpload([FromRoute] Guid fileId, [FromBody] ConfirmUploadBody body, CancellationToken cancellationToken)
    {
        var request = new ConfirmUploadRequest(fileId, body.EntityId);
        await mediaService.ConfirmUploadAsync(request);
        return NoContent();
    }
}
