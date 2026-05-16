using HiveSpace.CatalogService.Application.Categories.Queries.GetAttributesByCategoryId;
using HiveSpace.CatalogService.Application.Categories.Queries.GetCategories;
using HiveSpace.CatalogService.Application.Categories.Queries.GetHomepageCategories;
using MediatR;

namespace HiveSpace.CatalogService.Api.Endpoints;

public static class CategoryEndpoints
{
    public static IEndpointRouteBuilder MapCategoryEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/categories")
            .WithTags("Categories")
            .AllowAnonymous();

        group.MapGet("/", async (ISender sender) =>
        {
            var result = await sender.Send(new GetCategoriesQuery());
            return Results.Ok(result);
        })
        .WithName("GetCategories")
        .Produces(StatusCodes.Status200OK);

        group.MapGet("/homepage", async (ISender sender) =>
        {
            var result = await sender.Send(new GetHomepageCategoriesQuery());
            return Results.Ok(result);
        })
        .WithName("GetHomepageCategories")
        .Produces(StatusCodes.Status200OK);

        group.MapGet("/{categoryId:int}/attributes", async (
            int categoryId,
            ISender sender) =>
        {
            var result = await sender.Send(new GetAttributesByCategoryIdQuery(categoryId));
            return Results.Ok(result);
        })
        .WithName("GetCategoryAttributes")
        .Produces(StatusCodes.Status200OK);

        return app;
    }
}
