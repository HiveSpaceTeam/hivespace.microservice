using HiveSpace.CatalogService.Application.Contracts;
using HiveSpace.CatalogService.Application.Products.Commands.CreateProduct;
using HiveSpace.CatalogService.Application.Products.Commands.DeleteProduct;
using HiveSpace.CatalogService.Application.Products.Commands.UpdateProduct;
using HiveSpace.CatalogService.Application.Products.Queries.GetProduct;
using HiveSpace.CatalogService.Application.Products.Queries.GetProductDetail;
using HiveSpace.CatalogService.Application.Products.Queries.GetProductSummaries;
using HiveSpace.CatalogService.Application.Products.Queries.GetProducts;
using HiveSpace.Infrastructure.Authorization;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace HiveSpace.CatalogService.Api.Endpoints;

public static class ProductEndpoints
{
    public static IEndpointRouteBuilder MapProductEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/products")
            .WithTags("Products");

        group.MapPost("/", async (
            [FromBody] ProductUpsertRequestDto request,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var id = await mediator.Send(new CreateProductCommand(request), ct);
            return Results.Created($"/api/v1/products/{id}", id);
        })
        .RequireAuthorization(HiveSpaceAuthorizeAttribute.Seller.Policy)
        .WithName("CreateProduct")
        .Produces<int>(StatusCodes.Status201Created);

        group.MapPut("/{id:int}", async (
            int id,
            [FromBody] ProductUpsertRequestDto request,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var updated = await mediator.Send(new UpdateProductCommand(id, request), ct);
            return updated ? Results.NoContent() : Results.NotFound();
        })
        .RequireAuthorization(HiveSpaceAuthorizeAttribute.Seller.Policy)
        .WithName("UpdateProduct")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/", async (
            IMediator mediator,
            CancellationToken ct,
            [FromQuery] string keyword = "",
            [FromQuery] string sort = "ASC",
            [FromQuery] int pageSize = 10,
            [FromQuery] int page = 0) =>
        {
            var request = new ProductSearchRequestDto(keyword, sort, pageSize, page);
            var result = await mediator.Send(new GetProductsQuery(request), ct);
            return Results.Ok(result);
        })
        .RequireAuthorization(HiveSpaceAuthorizeAttribute.Seller.Policy)
        .WithName("GetProducts")
        .Produces(StatusCodes.Status200OK);

        group.MapGet("/{id:int}", async (
            int id,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var product = await mediator.Send(new GetProductQuery(id), ct);
            return Results.Ok(product);
        })
        .RequireAuthorization(HiveSpaceAuthorizeAttribute.Seller.Policy)
        .WithName("GetProduct")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:int}", async (
            int id,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var deleted = await mediator.Send(new DeleteProductCommand(id), ct);
            return deleted ? Results.NoContent() : Results.NotFound();
        })
        .RequireAuthorization(HiveSpaceAuthorizeAttribute.Seller.Policy)
        .WithName("DeleteProduct")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/summaries", async (
            IMediator mediator,
            CancellationToken ct,
            [FromQuery] string keyword = "",
            [FromQuery] string sort = "ASC",
            [FromQuery] int pageSize = 10,
            [FromQuery] int page = 0) =>
        {
            var request = new ProductSearchRequestDto(keyword, sort, pageSize, page);
            var result = await mediator.Send(new GetProductSummariesQuery(request), ct);
            return Results.Ok(result);
        })
        .AllowAnonymous()
        .WithName("GetProductSummaries")
        .Produces(StatusCodes.Status200OK);

        group.MapGet("/detail/{id:int}", async (
            int id,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var product = await mediator.Send(new GetProductDetailQuery(id), ct);
            return Results.Ok(product);
        })
        .AllowAnonymous()
        .WithName("GetProductDetail")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        return app;
    }
}
