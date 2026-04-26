using MediatR;
using Microsoft.AspNetCore.Mvc;
using HiveSpace.MediaService.Core.Features.Media.Commands.GeneratePresignedUrl;
using HiveSpace.MediaService.Core.Features.Media.Commands.ConfirmUpload;
using HiveSpace.MediaService.Core.Features.Media.Dtos;

namespace HiveSpace.MediaService.Api.Endpoints;

public static class MediaEndpoints
{
    public static IEndpointRouteBuilder MapMediaEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/v1/media")
            .RequireAuthorization();

        group.MapPost("presign-url", async (
            [FromBody] GeneratePresignedUrlCommand command,
            ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(command, ct);
            return Results.Ok(result);
        });

        group.MapPost("{fileId:guid}/confirm", async (
            Guid fileId,
            [FromBody] ConfirmUploadBody body,
            ISender sender,
            CancellationToken ct) =>
        {
            await sender.Send(new ConfirmUploadCommand(fileId, body.EntityId), ct);
            return Results.NoContent();
        });

        return app;
    }
}
