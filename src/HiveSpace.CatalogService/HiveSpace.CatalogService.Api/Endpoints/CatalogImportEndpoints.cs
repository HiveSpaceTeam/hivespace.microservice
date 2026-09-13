using HiveSpace.CatalogService.Application.CatalogImports.Commands.ApproveImportedSellerOwnership;
using HiveSpace.CatalogService.Application.CatalogImports.Commands.MapImportedCategory;
using HiveSpace.CatalogService.Application.CatalogImports.Commands.ProvisionImportedCategoryAttributes;
using HiveSpace.CatalogService.Application.CatalogImports.Commands.ProvisionImportedCategories;
using HiveSpace.CatalogService.Application.CatalogImports.Commands.SubmitCatalogImportBundle;
using HiveSpace.CatalogService.Application.CatalogImports.Commands.ProvisionImportedSellers;
using HiveSpace.CatalogService.Application.CatalogImports.Commands.ImportReadyProducts;
using HiveSpace.CatalogService.Application.CatalogImports.Commands.RetryCatalogImportJob;
using HiveSpace.CatalogService.Application.CatalogImports.Commands.ValidateCatalogImportBundle;
using HiveSpace.CatalogService.Application.CatalogImports.Dtos;
using HiveSpace.CatalogService.Application.CatalogImports.Queries.GetCatalogImportJob;
using HiveSpace.CatalogService.Application.CatalogImports.Queries.GetCatalogImportBundleDetail;
using HiveSpace.CatalogService.Application.CatalogImports.Queries.ListCatalogImportBundleSection;
using HiveSpace.CatalogService.Application.CatalogImports.Queries.ListCatalogImportBundles;
using HiveSpace.CatalogService.Application.CatalogImports.Queries.ListCatalogImportJobs;
using HiveSpace.Core.Contexts;
using HiveSpace.Infrastructure.Authorization;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace HiveSpace.CatalogService.Api.Endpoints;

public static class CatalogImportEndpoints
{
    public static IEndpointRouteBuilder MapCatalogImportEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/admins/catalog-imports")
            .WithTags("Admin Catalog Imports")
            .RequireAuthorization(HiveSpaceAuthorizeAttribute.Admin.Policy);

        group.MapPost("/bundles", async (
            [FromBody] CatalogImportBundleRequestDto request,
            [FromHeader(Name = "X-Source-File-Name")] string? sourceFileName,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var result = await mediator.Send(new SubmitCatalogImportBundleCommand(request, sourceFileName), ct);
            return Results.Accepted($"/api/v1/admins/catalog-imports/jobs/{result.JobId}", result);
        })
        .WithName("SubmitCatalogImportBundle")
        .Produces<CatalogImportJobSubmissionDto>(StatusCodes.Status202Accepted)
        .Produces(StatusCodes.Status400BadRequest);

        group.MapPost("/categories/provisioning", async (
            [FromBody] CategoryProvisioningRequestDto request,
            [FromHeader(Name = "X-Source-File-Name")] string? sourceFileName,
            IMediator mediator,
            IUserContext userContext,
            CancellationToken ct) =>
        {
            var result = await mediator.Send(new ProvisionImportedCategoriesCommand(request, userContext.UserId, sourceFileName), ct);
            return Results.Accepted($"/api/v1/admins/catalog-imports/jobs/{result.JobId}", result);
        })
        .WithName("ProvisionCatalogImportCategories")
        .Produces<CatalogImportJobSubmissionDto>(StatusCodes.Status202Accepted)
        .Produces(StatusCodes.Status400BadRequest);

        group.MapPost("/categories/attributes/provisioning", async (
            [FromBody] CategoryAttributeProvisioningRequestDto request,
            [FromHeader(Name = "X-Source-File-Name")] string? sourceFileName,
            IMediator mediator,
            IUserContext userContext,
            CancellationToken ct) =>
        {
            var result = await mediator.Send(new ProvisionImportedCategoryAttributesCommand(request, userContext.UserId, sourceFileName), ct);
            return Results.Accepted($"/api/v1/admins/catalog-imports/jobs/{result.JobId}", result);
        })
        .WithName("ProvisionCatalogImportCategoryAttributes")
        .Produces<CatalogImportJobSubmissionDto>(StatusCodes.Status202Accepted)
        .Produces(StatusCodes.Status400BadRequest);

        group.MapGet("/jobs", async (
            IMediator mediator,
            CancellationToken ct,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? searchTerm = null,
            [FromQuery] string? status = null,
            [FromQuery] string? operationType = null,
            [FromQuery] string? sourceSystem = null,
            [FromQuery] Guid? bundleId = null,
            [FromQuery] DateTimeOffset? requestedFrom = null,
            [FromQuery] DateTimeOffset? requestedTo = null) =>
        {
            var result = await mediator.Send(new ListCatalogImportJobsQuery(
                pageNumber,
                pageSize,
                searchTerm,
                status,
                operationType,
                sourceSystem,
                bundleId,
                requestedFrom,
                requestedTo),
                ct);
            return Results.Ok(result);
        })
        .WithName("ListCatalogImportJobs")
        .Produces<CatalogImportPagedResponseDto<CatalogImportJobDto>>(StatusCodes.Status200OK);

        group.MapGet("/jobs/{jobId:guid}", async (
            Guid jobId,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var result = await mediator.Send(new GetCatalogImportJobQuery(jobId), ct);
            return Results.Ok(result);
        })
        .WithName("GetCatalogImportJob")
        .Produces<CatalogImportJobDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/jobs/{jobId:guid}/retry", async (
            Guid jobId,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var result = await mediator.Send(new RetryCatalogImportJobCommand(jobId), ct);
            return Results.Accepted($"/api/v1/admins/catalog-imports/jobs/{result.JobId}", result);
        })
        .WithName("RetryCatalogImportJob")
        .Produces<CatalogImportJobSubmissionDto>(StatusCodes.Status202Accepted)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status409Conflict);

        group.MapGet("/bundles", async (
            IMediator mediator,
            CancellationToken ct,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20) =>
        {
            var result = await mediator.Send(new ListCatalogImportBundlesQuery(pageNumber, pageSize), ct);
            return Results.Ok(result);
        })
        .WithName("ListCatalogImportBundles")
        .Produces<CatalogImportPagedResponseDto<CatalogImportBundleSummaryDto>>(StatusCodes.Status200OK);

        group.MapGet("/bundles/{bundleId:guid}", async (
            Guid bundleId,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var result = await mediator.Send(new GetCatalogImportBundleDetailQuery(bundleId), ct);
            return Results.Ok(result);
        })
        .WithName("GetCatalogImportBundleDetail")
        .Produces<CatalogImportBundleSummaryDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/bundles/{bundleId:guid}/{section:regex(^(category-links|sellers|seller-ownership-candidates|products|duplicate-groups|validation-issues)$)}", async (
            Guid bundleId,
            string section,
            IMediator mediator,
            CancellationToken ct,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? searchTerm = null,
            [FromQuery] string? status = null,
            [FromQuery] string? sellerId = null,
            [FromQuery] string? severity = null,
            [FromQuery] string? entityType = null,
            [FromQuery] string? reasonCode = null) =>
        {
            var result = await mediator.Send(new ListCatalogImportBundleSectionQuery(
                bundleId,
                section,
                pageNumber,
                pageSize,
                searchTerm,
                status,
                sellerId,
                severity,
                entityType,
                reasonCode),
                ct);
            return Results.Ok(result);
        })
        .WithName("ListCatalogImportBundleSection")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/bundles/{bundleId:guid}/seller-provisioning", async (
            Guid bundleId,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var result = await mediator.Send(new ProvisionImportedSellersCommand(bundleId), ct);
            return Results.Accepted($"/api/v1/admins/catalog-imports/jobs/{result.JobId}", result);
        })
        .WithName("ProvisionCatalogImportSellers")
        .Produces<CatalogImportJobSubmissionDto>(StatusCodes.Status202Accepted)
        .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/bundles/{bundleId:guid}/sellers/{importedSellerId:guid}/ownership-link", async (
            Guid bundleId,
            Guid importedSellerId,
            [FromBody] ApproveImportedSellerOwnershipRequestDto request,
            IMediator mediator,
            IUserContext userContext,
            CancellationToken ct) =>
        {
            var result = await mediator.Send(new ApproveImportedSellerOwnershipCommand(
                bundleId,
                importedSellerId,
                request.TargetUserId,
                request.TargetStoreId,
                userContext.UserId,
                request.ApprovalReason),
                ct);
            return Results.Ok(result);
        })
        .WithName("ApproveImportedSellerOwnership")
        .Produces<ApproveImportedSellerOwnershipResultDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/bundles/{bundleId:guid}/category-links/{externalCategoryId}/mapping", async (
            Guid bundleId,
            string externalCategoryId,
            [FromBody] MapImportedCategoryRequestDto request,
            IMediator mediator,
            IUserContext userContext,
            CancellationToken ct) =>
        {
            var result = await mediator.Send(new MapImportedCategoryCommand(
                externalCategoryId,
                bundleId,
                request.HiveSpaceCategoryId,
                userContext.UserId),
                ct);
            return Results.Ok(result);
        })
        .WithName("MapCatalogImportCategoryLink")
        .Produces<MapImportedCategoryResultDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/bundles/{bundleId:guid}/validate", async (
            Guid bundleId,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var result = await mediator.Send(new ValidateCatalogImportBundleCommand(bundleId), ct);
            return Results.Accepted($"/api/v1/admins/catalog-imports/jobs/{result.JobId}", result);
        })
        .WithName("ValidateCatalogImportBundle")
        .Produces<CatalogImportJobSubmissionDto>(StatusCodes.Status202Accepted)
        .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/bundles/{bundleId:guid}/import", async (
            Guid bundleId,
            [FromBody] ImportReadyProductsRequestDto request,
            IMediator mediator,
            IUserContext userContext,
            CancellationToken ct) =>
        {
            var result = await mediator.Send(new ImportReadyProductsCommand(
                bundleId,
                request.ProductIds,
                request.PublicationState,
                userContext.UserId),
                ct);
            return Results.Accepted($"/api/v1/admins/catalog-imports/jobs/{result.JobId}", result);
        })
        .WithName("ImportCatalogImportReadyProducts")
        .Produces<CatalogImportJobSubmissionDto>(StatusCodes.Status202Accepted)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound);

        return app;
    }
}
