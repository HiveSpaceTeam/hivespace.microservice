using Asp.Versioning;
using Carter;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace HiveSpace.MediaService.Api.Features.ConfirmUpload;

public class ConfirmUploadEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/media/{id}/confirm", async (
            Guid id,
            [FromBody] ConfirmUploadRequest request,
            [FromServices] ISender sender) =>
        {
            var command = new ConfirmUploadCommand(id, request.EntityId);
            await sender.Send(command);
            return Results.NoContent();
        })
        .HasApiVersion(new ApiVersion(1.0))
        .WithTags("Media")
        .WithName("ConfirmUpload")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .WithOpenApi();
    }
}
