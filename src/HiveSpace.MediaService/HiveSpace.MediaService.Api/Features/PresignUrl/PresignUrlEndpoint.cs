using Asp.Versioning;
using Carter;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace HiveSpace.MediaService.Api.Features.PresignUrl;

public class PresignUrlEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/media/presign-url", async (
            [FromBody] PresignUrlCommand command,
            [FromServices] ISender sender) =>
        {
            var response = await sender.Send(command);
            return Results.Ok(response);
        })
        .HasApiVersion(new ApiVersion(1.0))
        .WithTags("Media")
        .WithName("PresignUrl")
        .WithOpenApi();
    }
}
