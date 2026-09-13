using HiveSpace.Infrastructure.Authorization;
using HiveSpace.UserService.Application.Stores.Commands.ProvisionImportedSellerStore;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace HiveSpace.UserService.Api.Endpoints;

public static class AdminStoreEndpoints
{
    public static IEndpointRouteBuilder MapAdminStoreEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/admins/imported-seller-stores", async (
            [FromBody] ProvisionImportedSellerStoreRequest request,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var result = await mediator.Send(new ProvisionImportedSellerStoreCommand(
                request.SourceSystem,
                request.ExternalSellerId,
                request.UserId,
                request.StoreName,
                request.SourceUrl,
                request.LogoUrl), ct);

            return Results.Ok(result);
        })
        .RequireAuthorization(HiveSpaceAuthorizeAttribute.CatalogImportProvisioning.Policy)
        .WithName("ProvisionImportedSellerStore")
        .WithTags("Admin Stores")
        .Produces<ProvisionImportedSellerStoreResult>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest);

        return app;
    }
}

public record ProvisionImportedSellerStoreRequest(
    string SourceSystem,
    string ExternalSellerId,
    Guid UserId,
    string StoreName,
    string? SourceUrl,
    string? LogoUrl = null);
