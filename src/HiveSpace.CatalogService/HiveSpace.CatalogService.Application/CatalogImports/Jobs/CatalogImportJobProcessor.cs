using HiveSpace.CatalogService.Application.CatalogImports.Dtos;
using HiveSpace.CatalogService.Application.CatalogImports.Commands.ImportReadyProducts;
using HiveSpace.CatalogService.Application.CatalogImports.Ports;
using HiveSpace.CatalogService.Application.Interfaces.Messaging;
using HiveSpace.CatalogService.Domain.Aggregates.External;
using HiveSpace.CatalogService.Domain.Aggregates.ProductAggregate;
using HiveSpace.CatalogService.Domain.Aggregates.CategoryAggregate;
using HiveSpace.CatalogService.Domain.CatalogImports;
using HiveSpace.CatalogService.Domain.CatalogImports.Enums;
using HiveSpace.CatalogService.Domain.Enums;
using HiveSpace.CatalogService.Domain.Exceptions;
using HiveSpace.CatalogService.Domain.Repositories;
using HiveSpace.CatalogService.Domain.Repositories.External;
using HiveSpace.Domain.Shared.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using HiveSpace.Domain.Shared.Enumerations;
using HiveSpace.Domain.Shared.ValueObjects;
using System.Text.Json;
using HiveSpace.CatalogService.Domain.Aggregates.AttributeAggregate;

namespace HiveSpace.CatalogService.Application.CatalogImports.Jobs;

public class CatalogImportJobProcessor(
    ICatalogImportBundleRepository repository,
    ICategoryRepository categoryRepository,
    ICatalogImportJobLifecyclePublisher lifecyclePublisher,
    IAttributeRepository? attributeRepository = null,
    IProductRepository? productRepository = null,
    IPlatformCurrencyPolicyRefRepository? currencyPolicyRepository = null,
    IImportedSellerAccountClient? accountClient = null,
    IImportedSellerStoreClient? storeClient = null,
    IStoreRefRepository? storeRefRepository = null,
    IProductEventPublisher? productEventPublisher = null,
    ILogger<CatalogImportJobProcessor>? logger = null)
    : ICatalogImportJobProcessor
{
    private const int CategoryAttributeHeartbeatInterval = 100;
    private const int ValidateBundleHeartbeatInterval = 25;
    private static readonly TimeSpan LifecyclePublishTimeout = TimeSpan.FromSeconds(10);
    private readonly ILogger<CatalogImportJobProcessor> _logger = logger ?? NullLogger<CatalogImportJobProcessor>.Instance;

    public async Task<int> ProcessPendingJobsAsync(int limit, CancellationToken cancellationToken = default)
    {
        var jobs = await repository.GetPendingJobsAsync(limit, cancellationToken);
        foreach (var job in jobs)
            await ProcessJobAsync(job.Id, cancellationToken);

        return jobs.Count;
    }

    public async Task<int> RecoverStaleRunningJobsAsync(
        TimeSpan staleAfter,
        int limit,
        CancellationToken cancellationToken = default)
    {
        var cutoff = DateTimeOffset.UtcNow - staleAfter;
        var jobs = await repository.GetRunningJobsInactiveSinceAsync(cutoff, limit, cancellationToken);

        foreach (var job in jobs)
        {
            try
            {
                if (job.Status != CatalogImportJobStatus.Running)
                    continue;

                job.Fail("Catalog import job stalled while running and was marked failed. Retry the job.");
                await repository.SaveChangesAsync(cancellationToken);
                await TryPublishFailedAsync(job, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to recover stale catalog import job {JobId}", job.Id);
            }
        }

        return jobs.Count;
    }

    public async Task ProcessJobAsync(Guid jobId, CancellationToken cancellationToken = default)
    {
        var job = await repository.GetJobByIdAsync(jobId, cancellationToken)
            ?? throw new NotFoundException(CatalogDomainErrorCode.CatalogImportJobNotFound, nameof(CatalogImportJob));

        try
        {
            var started = await repository.TryStartJobAsync(job.Id, cancellationToken);
            if (!started)
                return;

            if (job.Status == CatalogImportJobStatus.Pending)
                job.Start();

            await TryPublishStartedAsync(job, cancellationToken);

            switch (job.OperationType)
            {
                case CatalogImportJobOperationType.SubmitBundle:
                    await ProcessSubmitBundleAsync(job, cancellationToken);
                    break;
                case CatalogImportJobOperationType.ProvisionCategories:
                    await ProcessProvisionCategoriesAsync(job, cancellationToken);
                    break;
                case CatalogImportJobOperationType.ProvisionCategoryAttributes:
                    await ProcessProvisionCategoryAttributesAsync(job, cancellationToken);
                    break;
                case CatalogImportJobOperationType.ValidateBundle:
                    await ProcessValidateBundleAsync(job, cancellationToken);
                    break;
                case CatalogImportJobOperationType.ProvisionSellers:
                    await ProcessProvisionSellersAsync(job, cancellationToken);
                    break;
                case CatalogImportJobOperationType.ImportReadyProducts:
                    await ProcessImportReadyProductsAsync(job, cancellationToken);
                    break;
                default:
                    throw new InvalidFieldException(CatalogDomainErrorCode.InvalidCatalogImportJob, nameof(job.OperationType));
            }

            await repository.SaveChangesAsync(cancellationToken);
            await TryPublishCompletedAsync(job, cancellationToken);
        }
        catch (DomainException ex)
        {
            await FailJobAsync(job, ex.Message, cancellationToken);
        }
        catch (JsonException)
        {
            await FailJobAsync(job, "Catalog import job payload is invalid.", cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Catalog import job {JobId} failed unexpectedly", job.Id);
            await FailJobAsync(
                job,
                "Catalog import job failed due to an unexpected infrastructure error.",
                cancellationToken);
        }
    }

    private async Task ProcessSubmitBundleAsync(CatalogImportJob job, CancellationToken cancellationToken)
    {
        var payload = Deserialize<CatalogImportBundleRequestDto>(job.RequestPayloadJson, nameof(CatalogImportBundleRequestDto));
        var existing = await repository.GetBySourceFingerprintAsync(payload.Crawl.SourceFingerprint, cancellationToken);
        if (existing is not null)
        {
            job.UpdateProgress(existing.TotalProducts, existing.TotalProducts, skipped: existing.TotalProducts, blocked: existing.BlockedProducts, warnings: existing.WarningCount, duplicates: existing.DuplicateCount);
            await lifecyclePublisher.PublishProgressedAsync(job, cancellationToken);
            job.Complete(CreateResultSummary(new
            {
                bundleId = existing.Id,
                status = existing.Status.ToString(),
                existing.TotalProducts,
                existing.ReadyProducts,
                existing.BlockedProducts,
                existing.WarningCount,
                existing.DuplicateCount
            }), existing.Id);
            return;
        }

        var bundle = CatalogImportBundle.Create(
            payload.SchemaVersion,
            payload.Source.System,
            payload.Source.Type,
            payload.Source.Value,
            payload.Crawl.SourceFingerprint,
            payload.Crawl.CompletedAt,
            job.RequestedByUserId,
            sourceUrl: payload.Source.Url,
            sourceFileName: job.SourceFileName,
            checkpointId: payload.Crawl.CheckpointId);

        AddSellers(bundle, payload.Sellers);
        await AddCategoryLinksAsync(bundle, payload, cancellationToken);
        AddProducts(bundle, payload.Products);
        AddDuplicateGroups(bundle);
        AddValidationHints(bundle, payload.ValidationHints);
        bundle.RefreshSummary();

        repository.Add(bundle);
        job.UpdateProgress(
            bundle.TotalProducts,
            bundle.TotalProducts,
            created: bundle.TotalProducts,
            blocked: bundle.BlockedProducts,
            warnings: bundle.WarningCount,
            duplicates: bundle.DuplicateCount);
        await TryPublishProgressedAsync(job, cancellationToken);
        job.Complete(CreateResultSummary(new
        {
            bundleId = bundle.Id,
            status = bundle.Status.ToString(),
            bundle.TotalProducts,
            bundle.ReadyProducts,
            bundle.BlockedProducts,
            bundle.WarningCount,
            bundle.DuplicateCount
        }), bundle.Id);
    }

    private async Task ProcessProvisionCategoriesAsync(CatalogImportJob job, CancellationToken cancellationToken)
    {
        var payload = Deserialize<CategoryProvisioningRequestDto>(job.RequestPayloadJson, nameof(CategoryProvisioningRequestDto));
        var categories = (await categoryRepository.GetAllAsync()).ToList();
        var nextCategoryId = categories.Count == 0 ? 1 : categories.Max(x => x.Id) + 1;
        var results = new List<CategoryProvisioningCategoryResultDto>();
        var pendingCategories = payload.Categories.ToDictionary(x => x.ExternalCategoryId, StringComparer.OrdinalIgnoreCase);
        var resolvedCategoryIds = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        while (pendingCategories.Count > 0)
        {
            var progressed = false;

            foreach (var category in pendingCategories.Values.ToList())
            {
                if (!CanResolveParent(category, resolvedCategoryIds, pendingCategories))
                    continue;

                var pathJson = category.Path is null ? null : JsonSerializer.Serialize(category.Path);
                var existingLink = await repository.GetExternalCategoryLinkAsync(
                    payload.Source.System,
                    category.ExternalCategoryId,
                    cancellationToken);

                if (existingLink is not null)
                {
                    await ApplyCategoryImageAsync(existingLink.HiveSpaceCategoryId, category);
                    existingLink.MarkMatched(
                        category.Name,
                        category.ExternalParentCategoryId,
                        pathJson,
                        existingLink.HiveSpaceCategoryId,
                        payload.Crawl.SourceFingerprint,
                        job.RequestedByUserId);
                    results.Add(new CategoryProvisioningCategoryResultDto(category.ExternalCategoryId, existingLink.HiveSpaceCategoryId, "Matched", null));
                    resolvedCategoryIds[category.ExternalCategoryId] = existingLink.HiveSpaceCategoryId;
                    pendingCategories.Remove(category.ExternalCategoryId);
                    progressed = true;
                    continue;
                }

                var parentId = await ResolveParentIdAsync(
                    payload.Source.System,
                    category.ExternalParentCategoryId,
                    resolvedCategoryIds,
                    cancellationToken);
                if (!string.IsNullOrWhiteSpace(category.ExternalParentCategoryId) && parentId is null)
                {
                    results.Add(new CategoryProvisioningCategoryResultDto(
                        category.ExternalCategoryId,
                        null,
                        "Failed",
                        $"UnresolvedParentCategory:{category.ExternalParentCategoryId}"));
                    pendingCategories.Remove(category.ExternalCategoryId);
                    progressed = true;
                    continue;
                }

                var existingCategory = categories.FirstOrDefault(x =>
                    string.Equals(x.Name, category.Name, StringComparison.OrdinalIgnoreCase)
                    && x.ParentId == parentId);
                var status = "Matched";

                if (existingCategory is null)
                {
                    existingCategory = new Category(
                        nextCategoryId++,
                        category.Name,
                        parentId,
                        ParseProductSetId(category.ProductSetId),
                        isActive: true,
                        Normalize(category.ImageFileId));
                    SetImageUrl(existingCategory, category.ImageUrl);
                    await categoryRepository.AddAsync(existingCategory);
                    categories.Add(existingCategory);
                    status = "Created";
                }
                else
                {
                    ApplyCategoryImage(existingCategory, category);
                }

                repository.AddExternalCategoryLink(ExternalCategoryLink.Create(
                    payload.Source.System,
                    category.ExternalCategoryId,
                    category.Name,
                    category.ExternalParentCategoryId,
                    pathJson,
                    existingCategory.Id,
                    payload.Crawl.SourceFingerprint,
                    job.RequestedByUserId));

                results.Add(new CategoryProvisioningCategoryResultDto(category.ExternalCategoryId, existingCategory.Id, status, null));
                resolvedCategoryIds[category.ExternalCategoryId] = existingCategory.Id;
                pendingCategories.Remove(category.ExternalCategoryId);
                progressed = true;
            }

            if (progressed)
                continue;

            foreach (var category in pendingCategories.Values)
            {
                results.Add(new CategoryProvisioningCategoryResultDto(
                    category.ExternalCategoryId,
                    null,
                    "Failed",
                    $"UnresolvedParentCategory:{category.ExternalParentCategoryId}"));
            }

            break;
        }

        var summary = new CategoryProvisioningSummaryDto(
            payload.Categories.Count,
            results.Count(x => x.Status == "Created"),
            results.Count(x => x.Status == "Matched"),
            results.Count(x => x.Status == "Failed"),
            results.Count(x => x.Status == "Conflict"));
        job.UpdateProgress(
            summary.TotalCategories,
            results.Count,
            created: summary.Created,
            matched: summary.Matched,
            failed: summary.Failed,
            conflicts: summary.Conflict);
        await TryPublishProgressedAsync(job, cancellationToken);
        job.Complete(CreateResultSummary(new CategoryProvisioningResultDto(
            "Completed",
            payload.Crawl.SourceFingerprint,
            summary,
            results)));
    }

    private async Task ProcessProvisionCategoryAttributesAsync(CatalogImportJob job, CancellationToken cancellationToken)
    {
        var payload = Deserialize<CategoryAttributeProvisioningRequestDto>(job.RequestPayloadJson, nameof(CategoryAttributeProvisioningRequestDto));
        var totalAttributes = payload.Categories.Sum(x => x.Attributes.Count);
        var attributeNames = payload.Categories
            .SelectMany(x => x.Attributes)
            .Select(x => x.Name)
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        var created = 0;
        var matched = 0;
        var failed = 0;
        var processed = 0;
        var lastPersistedProcessed = 0;

        lastPersistedProcessed = await PersistCategoryAttributeProgressAsync(
            job,
            totalAttributes,
            processed,
            created,
            matched,
            failed,
            lastPersistedProcessed,
            force: true,
            allowNoProgress: true,
            cancellationToken);

        var allCategories = (await categoryRepository.GetAllAsync()).ToList();
        lastPersistedProcessed = await PersistCategoryAttributeProgressAsync(
            job,
            totalAttributes,
            processed,
            created,
            matched,
            failed,
            lastPersistedProcessed,
            force: true,
            allowNoProgress: true,
            cancellationToken);

        var allAttributes = (await Required(attributeRepository, nameof(attributeRepository)).GetByNamesAsync(attributeNames)).ToList();
        var externalCategoryIds = payload.Categories.Select(x => x.ExternalCategoryId).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        lastPersistedProcessed = await PersistCategoryAttributeProgressAsync(
            job,
            totalAttributes,
            processed,
            created,
            matched,
            failed,
            lastPersistedProcessed,
            force: true,
            allowNoProgress: true,
            cancellationToken);

        var categoryLinks = (await repository.GetExternalCategoryLinksAsync(payload.Source.System, externalCategoryIds, cancellationToken))
            .ToDictionary(x => x.ExternalCategoryId, StringComparer.OrdinalIgnoreCase);
        lastPersistedProcessed = await PersistCategoryAttributeProgressAsync(
            job,
            totalAttributes,
            processed,
            created,
            matched,
            failed,
            lastPersistedProcessed,
            force: true,
            allowNoProgress: true,
            cancellationToken);

        var existingAttributeLinks = (await repository.GetExternalCategoryAttributeLinksAsync(payload.Source.System, externalCategoryIds, cancellationToken))
            .ToDictionary(x => $"{x.ExternalCategoryId}::{x.SourceAttributeId}", StringComparer.OrdinalIgnoreCase);
        lastPersistedProcessed = await PersistCategoryAttributeProgressAsync(
            job,
            totalAttributes,
            processed,
            created,
            matched,
            failed,
            lastPersistedProcessed,
            force: true,
            allowNoProgress: true,
            cancellationToken);

        foreach (var categoryPayload in payload.Categories)
        {
            if (!categoryLinks.TryGetValue(categoryPayload.ExternalCategoryId, out var categoryLink))
            {
                failed += categoryPayload.Attributes.Count;
                processed += categoryPayload.Attributes.Count;
                lastPersistedProcessed = await PersistCategoryAttributeProgressAsync(
                    job,
                    totalAttributes,
                    processed,
                    created,
                    matched,
                    failed,
                    lastPersistedProcessed,
                    force: true,
                    allowNoProgress: false,
                    cancellationToken);
                continue;
            }

            var category = allCategories.FirstOrDefault(x => x.Id == categoryLink.HiveSpaceCategoryId);
            if (category is null)
            {
                failed += categoryPayload.Attributes.Count;
                processed += categoryPayload.Attributes.Count;
                lastPersistedProcessed = await PersistCategoryAttributeProgressAsync(
                    job,
                    totalAttributes,
                    processed,
                    created,
                    matched,
                    failed,
                    lastPersistedProcessed,
                    force: true,
                    allowNoProgress: false,
                    cancellationToken);
                continue;
            }

            foreach (var attributePayload in categoryPayload.Attributes)
            {
                processed++;
                var attributeType = CreateAttributeType(attributePayload.InputType, attributePayload.IsRequired, attributePayload.Values.Count > 0);
                var existingAttribute = allAttributes.FirstOrDefault(x =>
                    string.Equals(x.Name, attributePayload.Name, StringComparison.OrdinalIgnoreCase));
                var hasNewSelectableValues = false;
                var valueMappings = new List<(string SourceValueId, AttributeValue Value)>();

                if (existingAttribute is null)
                {
                    existingAttribute = new AttributeDefinition(
                        attributePayload.Name,
                        attributeType);
                    await Required(attributeRepository, nameof(attributeRepository)).AddAsync(existingAttribute);
                    await repository.SaveChangesAsync(cancellationToken);
                    allAttributes.Add(existingAttribute);
                    created++;
                }
                else
                {
                    existingAttribute.UpdateDefinition(attributePayload.Name, attributeType, true);
                    await Required(attributeRepository, nameof(attributeRepository)).UpdateAsync(existingAttribute);
                    matched++;
                }

                if (!category.CategoryAttributes.Any(x => x.AttributeId == existingAttribute.Id))
                    category.AddAttribute(existingAttribute.Id);
                await categoryRepository.UpdateAsync(category);

                var selectableValueLinks = new List<ExternalCategoryAttributeValueLink>();
                var sortOrder = 1;
                foreach (var valuePayload in attributePayload.Values)
                {
                    var existingValue = existingAttribute.FindValue(valuePayload.Name, valuePayload.DisplayName);
                    if (existingValue is null)
                    {
                        existingValue = existingAttribute.AddValue(
                            valuePayload.Name,
                            valuePayload.DisplayName,
                            sortOrder: sortOrder++);
                        hasNewSelectableValues = true;
                    }
                    else
                    {
                        existingValue.Update(valuePayload.Name, valuePayload.DisplayName, true, sortOrder++);
                    }

                    valueMappings.Add((valuePayload.SourceValueId, existingValue));
                }

                if (hasNewSelectableValues)
                {
                    await Required(attributeRepository, nameof(attributeRepository)).UpdateAsync(existingAttribute);
                    await repository.SaveChangesAsync(cancellationToken);
                }

                foreach (var (sourceValueId, value) in valueMappings)
                {
                    selectableValueLinks.Add(new ExternalCategoryAttributeValueLink(
                        sourceValueId,
                        value.Name,
                        value.DisplayName,
                        value.Id));
                }

                var linkKey = $"{categoryPayload.ExternalCategoryId}::{attributePayload.SourceAttributeId}";
                if (existingAttributeLinks.TryGetValue(linkKey, out var existingLink))
                {
                    existingLink.Update(
                        attributePayload.Name,
                        attributePayload.InputType,
                        attributePayload.IsRequired,
                        category.Id,
                        existingAttribute.Id,
                        selectableValueLinks,
                        payload.Crawl.SourceFingerprint,
                        job.RequestedByUserId);
                }
                else
                {
                    repository.AddExternalCategoryAttributeLink(ExternalCategoryAttributeLink.Create(
                        payload.Source.System,
                        categoryPayload.ExternalCategoryId,
                        attributePayload.SourceAttributeId,
                        attributePayload.Name,
                        attributePayload.InputType,
                        attributePayload.IsRequired,
                        category.Id,
                        existingAttribute.Id,
                        selectableValueLinks,
                        payload.Crawl.SourceFingerprint,
                        job.RequestedByUserId));
                }

                lastPersistedProcessed = await PersistCategoryAttributeProgressAsync(
                    job,
                    totalAttributes,
                    processed,
                    created,
                    matched,
                    failed,
                    lastPersistedProcessed,
                    force: processed % CategoryAttributeHeartbeatInterval == 0,
                    allowNoProgress: false,
                    cancellationToken);
            }

            lastPersistedProcessed = await PersistCategoryAttributeProgressAsync(
                job,
                totalAttributes,
                processed,
                created,
                matched,
                failed,
                lastPersistedProcessed,
                force: true,
                allowNoProgress: false,
                cancellationToken);
        }

        await PersistCategoryAttributeProgressAsync(
            job,
            totalAttributes,
            processed,
            created,
            matched,
            failed,
            lastPersistedProcessed,
            force: true,
            allowNoProgress: false,
            cancellationToken);
        job.Complete(CreateResultSummary(new
        {
            status = "Completed",
            payload.Crawl.SourceFingerprint,
            totalAttributes,
            created,
            matched,
            failed
        }));
    }

    private async Task<int> PersistCategoryAttributeProgressAsync(
        CatalogImportJob job,
        int total,
        int processed,
        int created,
        int matched,
        int failed,
        int lastPersistedProcessed,
        bool force,
        bool allowNoProgress,
        CancellationToken cancellationToken)
    {
        if (!force && processed - lastPersistedProcessed < CategoryAttributeHeartbeatInterval)
            return lastPersistedProcessed;

        if (!allowNoProgress && processed == lastPersistedProcessed)
            return lastPersistedProcessed;

        job.UpdateProgress(
            total,
            processed,
            created: created,
            matched: matched,
            failed: failed);
        await repository.SaveChangesAsync(cancellationToken);
        await TryPublishProgressedAsync(job, cancellationToken);
        return processed;
    }

    private async Task ProcessValidateBundleAsync(CatalogImportJob job, CancellationToken cancellationToken)
    {
        var bundle = await GetJobBundleAsync(job, cancellationToken);
        var currencyPolicy = await Required(currencyPolicyRepository, nameof(currencyPolicyRepository)).GetCurrentAsync(cancellationToken);
        var vndEnabled = currencyPolicy?.IsCurrencyEnabled("VND") == true;
        EnsureMissingCategoryPlaceholders(
            bundle,
            bundle.Products.SelectMany(product => product.ExternalCategoryIds));
        var attributeLinks = await repository.GetExternalCategoryAttributeLinksAsync(
            bundle.SourceSystem,
            bundle.CategoryMappings.Select(x => x.ExternalCategoryId),
            cancellationToken);
        var issues = CatalogImportValidator.Validate(bundle, vndEnabled, attributeLinks).ToList();

        var products = bundle.Products.ToList();
        var existingProducts = await Required(productRepository, nameof(productRepository))
            .GetAllAsync(cancellationToken);
        var processed = 0;

        foreach (var product in products)
        {
            var existingImportedProductId = await repository.GetImportedProductIdBySourceIdentityAsync(
                bundle.SourceSystem,
                product.ExternalProductId,
                cancellationToken);
            if (existingImportedProductId.HasValue)
            {
                product.MarkImported(existingImportedProductId.Value);
                processed++;
                if (processed % ValidateBundleHeartbeatInterval == 0)
                    await PersistValidateBundleProgressAsync(job, products.Count, processed, cancellationToken);
                continue;
            }

            var similarProducts = existingProducts
                .Where(existingProduct => CatalogImportSimilarity.IsSimilarName(existingProduct.Name, product.Title))
                .ToList();
            if (similarProducts.Count > 0)
            {
                issues.Add(ImportValidationIssue.Create(
                    bundle.Id,
                    "Product",
                    product.ExternalProductId,
                    "title",
                    ImportValidationSeverity.Blocking,
                    "ExistingProductDuplicateRisk",
                    $"Imported product '{product.Title}' is similar to an existing HiveSpace product and requires operator duplicate review."));
            }

            processed++;
            if (processed % ValidateBundleHeartbeatInterval == 0)
                await PersistValidateBundleProgressAsync(job, products.Count, processed, cancellationToken);
        }

        await repository.PrepareValidationIssueReplacementAsync(bundle.Id, cancellationToken);
        bundle.ReplaceValidationIssues(issues);
        await repository.DetachDeletedValidationIssuesAsync(bundle.Id, cancellationToken);
        job.UpdateProgress(
            bundle.TotalProducts,
            bundle.TotalProducts,
            blocked: bundle.BlockedProducts,
            warnings: bundle.WarningCount,
            duplicates: bundle.DuplicateCount);
        await TryPublishProgressedAsync(job, cancellationToken);
        job.Complete(CreateResultSummary(new ValidateCatalogImportBundleResultDto(
            bundle.Id,
            bundle.Status.ToString(),
            bundle.TotalProducts,
            bundle.ReadyProducts,
            bundle.BlockedProducts,
            bundle.WarningCount,
            bundle.DuplicateCount)), bundle.Id);
    }

    private async Task PersistValidateBundleProgressAsync(
        CatalogImportJob job,
        int total,
        int processed,
        CancellationToken cancellationToken)
    {
        job.UpdateProgress(total, processed);
        await repository.SaveJobProgressAsync(job.Id, total, processed, cancellationToken);
        await TryPublishProgressedAsync(job, cancellationToken);
    }

    private async Task ProcessProvisionSellersAsync(CatalogImportJob job, CancellationToken cancellationToken)
    {
        var bundle = await GetJobBundleAsync(job, cancellationToken);
        var created = 0;
        var matched = 0;
        var skipped = 0;
        var conflicts = 0;
        var failed = 0;
        var processed = 0;

        job.UpdateProgress(bundle.Sellers.Count, processed);
        await repository.SaveChangesAsync(cancellationToken);
        await TryPublishProgressedAsync(job, cancellationToken);

        foreach (var seller in bundle.Sellers)
        {
            if (seller.HasActiveOwnership)
            {
                skipped++;
                processed++;
                await PersistProvisionSellersProgressAsync(job, bundle, processed, created, matched, skipped, failed, conflicts, cancellationToken);
                continue;
            }

            seller.MarkCreateRequested();
            await repository.SaveChangesAsync(cancellationToken);
            var account = await Required(accountClient, nameof(accountClient)).ProvisionAsync(
                new ImportedSellerAccountProvisioningRequest(
                    bundle.SourceSystem,
                    seller.ExternalSellerId,
                    seller.DisplayName,
                    seller.SourceUrl),
                cancellationToken);

            if (account.Outcome is ImportedSellerProvisioningOutcome.Conflict or ImportedSellerProvisioningOutcome.Failed || account.UserId is null)
            {
                MarkSellerProblem(bundle, seller, account.Outcome, account.ConflictReason ?? "ImportedSellerAccountProvisioningFailed");
                if (account.Outcome == ImportedSellerProvisioningOutcome.Conflict)
                    conflicts++;
                else
                    failed++;
                processed++;
                await PersistProvisionSellersProgressAsync(job, bundle, processed, created, matched, skipped, failed, conflicts, cancellationToken);
                continue;
            }

            var store = await Required(storeClient, nameof(storeClient)).ProvisionAsync(
                new ImportedSellerStoreProvisioningRequest(
                    bundle.SourceSystem,
                    seller.ExternalSellerId,
                    account.UserId.Value,
                    seller.DisplayName,
                    seller.SourceUrl,
                    seller.LogoUrl),
                cancellationToken);

            if (store.Outcome is ImportedSellerProvisioningOutcome.Conflict or ImportedSellerProvisioningOutcome.Failed || store.StoreId is null)
            {
                MarkSellerProblem(bundle, seller, store.Outcome, store.ConflictReason ?? "ImportedSellerStoreProvisioningFailed");
                if (store.Outcome == ImportedSellerProvisioningOutcome.Conflict)
                    conflicts++;
                else
                    failed++;
                processed++;
                await PersistProvisionSellersProgressAsync(job, bundle, processed, created, matched, skipped, failed, conflicts, cancellationToken);
                continue;
            }

            var createdOwnership = account.Outcome == ImportedSellerProvisioningOutcome.Created
                || store.Outcome == ImportedSellerProvisioningOutcome.Created;
            await EnsureStoreRefAsync(
                bundle,
                seller,
                account.UserId.Value,
                store.StoreId.Value,
                cancellationToken);
            seller.MarkProvisioned(account.UserId.Value, store.StoreId.Value, createdOwnership);
            if (createdOwnership)
                created++;
            else
                matched++;
            processed++;
            await PersistProvisionSellersProgressAsync(job, bundle, processed, created, matched, skipped, failed, conflicts, cancellationToken);
        }

        bundle.RefreshSummary();
        var result = new ProvisionImportedSellersResultDto(bundle.Id, created, matched, skipped, conflicts, failed);
        job.UpdateProgress(
            bundle.Sellers.Count,
            bundle.Sellers.Count,
            created: created,
            matched: matched,
            skipped: skipped,
            failed: failed,
            conflicts: conflicts);
        await TryPublishProgressedAsync(job, cancellationToken);
        job.Complete(CreateResultSummary(result), bundle.Id);
    }

    private async Task PersistProvisionSellersProgressAsync(
        CatalogImportJob job,
        CatalogImportBundle bundle,
        int processed,
        int created,
        int matched,
        int skipped,
        int failed,
        int conflicts,
        CancellationToken cancellationToken)
    {
        bundle.RefreshSummary();
        job.UpdateProgress(
            bundle.Sellers.Count,
            processed,
            created: created,
            matched: matched,
            skipped: skipped,
            failed: failed,
            conflicts: conflicts);
        await repository.SaveChangesAsync(cancellationToken);
        await TryPublishProgressedAsync(job, cancellationToken);
    }

    private async Task ProcessImportReadyProductsAsync(CatalogImportJob job, CancellationToken cancellationToken)
    {
        var payload = Deserialize<ImportReadyProductsJobPayload>(job.RequestPayloadJson, nameof(ImportReadyProductsJobPayload));
        var bundle = await GetJobBundleAsync(job, cancellationToken);
        var selectedIds = payload.ProductIds.ToHashSet();
        var importedProducts = new List<Product>();
        var imported = 0;
        var skipped = 0;
        var blocked = 0;
        var failed = 0;

        foreach (var importedProduct in bundle.Products.Where(x => selectedIds.Contains(x.Id)))
        {
            if (!CanImport(bundle, importedProduct))
            {
                importedProduct.MarkSkipped();
                blocked++;
                continue;
            }

            try
            {
                var product = CreateProduct(bundle, importedProduct, payload.ImportedByUserId, payload.PublicationState);
                await Required(productRepository, nameof(productRepository)).AddAsync(product, cancellationToken);
                await Required(productRepository, nameof(productRepository)).SaveChangesAsync(cancellationToken);
                importedProduct.MarkImported(product.Id);
                importedProducts.Add(product);
                imported++;
            }
            catch (DomainException)
            {
                importedProduct.MarkFailed();
                failed++;
            }
        }

        bundle.RefreshImportStatus();
        if (importedProducts.Count > 0)
        {
            await Required(productEventPublisher, nameof(productEventPublisher))
                .PublishImportedProductsReplicaSyncAsync(bundle, importedProducts, cancellationToken);
        }

        var result = new ImportReadyProductsResultDto(bundle.Id, imported, skipped, blocked, failed);
        job.UpdateProgress(
            selectedIds.Count,
            imported + skipped + blocked + failed,
            created: imported,
            skipped: skipped,
            blocked: blocked,
            failed: failed);
        await TryPublishProgressedAsync(job, cancellationToken);
        job.Complete(CreateResultSummary(result), bundle.Id);
    }

    private async Task FailJobAsync(CatalogImportJob job, string errorSummary, CancellationToken cancellationToken)
    {
        try
        {
            await repository.MarkJobFailedAsync(job.Id, errorSummary, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to persist failed state for catalog import job {JobId}", job.Id);
        }

        await TryPublishFailedAsync(job, cancellationToken);
    }

    private async Task TryPublishStartedAsync(CatalogImportJob job, CancellationToken cancellationToken)
    {
        try
        {
            await PublishWithTimeoutAsync(
                token => lifecyclePublisher.PublishStartedAsync(job, token),
                "started",
                job,
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to publish started event for catalog import job {JobId}", job.Id);
        }
    }

    private async Task TryPublishProgressedAsync(CatalogImportJob job, CancellationToken cancellationToken)
    {
        try
        {
            await PublishWithTimeoutAsync(
                token => lifecyclePublisher.PublishProgressedAsync(job, token),
                "progress",
                job,
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to publish progress event for catalog import job {JobId}", job.Id);
        }
    }

    private async Task TryPublishCompletedAsync(CatalogImportJob job, CancellationToken cancellationToken)
    {
        try
        {
            await PublishWithTimeoutAsync(
                token => lifecyclePublisher.PublishCompletedAsync(job, token),
                "completed",
                job,
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to publish completed event for catalog import job {JobId}", job.Id);
        }
    }

    private async Task TryPublishFailedAsync(CatalogImportJob job, CancellationToken cancellationToken)
    {
        try
        {
            await PublishWithTimeoutAsync(
                token => lifecyclePublisher.PublishFailedAsync(job, token),
                "failed",
                job,
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to publish failed event for catalog import job {JobId}", job.Id);
        }
    }

    private async Task PublishWithTimeoutAsync(
        Func<CancellationToken, Task> publish,
        string lifecycleStage,
        CatalogImportJob job,
        CancellationToken cancellationToken)
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(LifecyclePublishTimeout);

        try
        {
            await publish(timeout.Token);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning(
                "Timed out publishing {LifecycleStage} event for catalog import job {JobId}",
                lifecycleStage,
                job.Id);
        }
    }

    private async Task EnsureStoreRefAsync(
        CatalogImportBundle bundle,
        ImportedSeller seller,
        Guid ownerId,
        Guid storeId,
        CancellationToken cancellationToken)
    {
        var repository = storeRefRepository;
        if (repository is null)
            return;

        var existing = await repository.GetByIdAsync(storeId, cancellationToken);
        if (existing is not null)
            return;

        var now = DateTimeOffset.UtcNow;
        await repository.AddAsync(
            new StoreRef(
                storeId,
                ownerId,
                seller.DisplayName,
                $"Imported seller store from {bundle.SourceSystem}. Source: {seller.SourceUrl ?? seller.ExternalSellerId}",
                seller.LogoUrl,
                $"Imported seller address pending review ({bundle.SourceSystem}:{seller.ExternalSellerId})",
                now,
                now),
            cancellationToken);
    }

    private async Task<CatalogImportBundle> GetJobBundleAsync(CatalogImportJob job, CancellationToken cancellationToken)
    {
        if (!job.BundleId.HasValue)
            throw new InvalidFieldException(CatalogDomainErrorCode.InvalidCatalogImportJob, nameof(job.BundleId));

        return await repository.GetByIdAsync(job.BundleId.Value, cancellationToken)
            ?? throw new NotFoundException(CatalogDomainErrorCode.CatalogImportBundleNotFound, nameof(CatalogImportBundle));
    }

    private async Task AddCategoryLinksAsync(
        CatalogImportBundle bundle,
        CatalogImportBundleRequestDto payload,
        CancellationToken cancellationToken)
    {
        var externalCategoryIds = payload.Products
            .SelectMany(x => x.ExternalCategoryIds)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        var links = await repository.GetExternalCategoryLinksAsync(
            payload.Source.System,
            externalCategoryIds,
            cancellationToken);

        foreach (var link in links)
        {
            bundle.AddCategoryLink(
                link.ExternalCategoryId,
                link.ExternalParentCategoryId,
                link.ExternalCategoryName,
                link.HiveSpaceCategoryId,
                link.PathJson);
        }

        EnsureMissingCategoryPlaceholders(bundle, externalCategoryIds);
    }

    private static void EnsureMissingCategoryPlaceholders(
        CatalogImportBundle bundle,
        IEnumerable<string> externalCategoryIds)
    {
        var linkedCategoryIds = bundle.CategoryMappings
            .Select(x => x.ExternalCategoryId)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        foreach (var externalCategoryId in externalCategoryIds
                     .Distinct(StringComparer.OrdinalIgnoreCase)
                     .Where(id => !linkedCategoryIds.Contains(id)))
        {
            bundle.AddCategory(
                externalCategoryId,
                externalParentCategoryId: null,
                $"External category {externalCategoryId}",
                pathJson: null);
            linkedCategoryIds.Add(externalCategoryId);
        }
    }

    private async Task<int?> ResolveParentIdAsync(
        string sourceSystem,
        string? externalParentCategoryId,
        IReadOnlyDictionary<string, int> resolvedCategoryIds,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(externalParentCategoryId))
            return null;

        if (resolvedCategoryIds.TryGetValue(externalParentCategoryId, out var resolvedParentId))
            return resolvedParentId;

        var parentLink = await repository.GetExternalCategoryLinkAsync(sourceSystem, externalParentCategoryId, cancellationToken);
        return parentLink?.HiveSpaceCategoryId;
    }

    private static bool CanResolveParent(
        CategoryProvisioningCategoryDto category,
        IReadOnlyDictionary<string, int> resolvedCategoryIds,
        IReadOnlyDictionary<string, CategoryProvisioningCategoryDto> pendingCategories)
    {
        if (string.IsNullOrWhiteSpace(category.ExternalParentCategoryId))
            return true;

        if (resolvedCategoryIds.ContainsKey(category.ExternalParentCategoryId))
            return true;

        return !pendingCategories.ContainsKey(category.ExternalParentCategoryId);
    }

    private async Task ApplyCategoryImageAsync(
        int categoryId,
        CategoryProvisioningCategoryDto category)
    {
        var existingCategory = await categoryRepository.GetByIdAsync(categoryId);
        if (existingCategory is null)
            return;

        ApplyCategoryImage(existingCategory, category);
        await categoryRepository.UpdateAsync(existingCategory);
    }

    private static void MarkSellerProblem(
        CatalogImportBundle bundle,
        ImportedSeller seller,
        ImportedSellerProvisioningOutcome outcome,
        string reason)
    {
        if (outcome == ImportedSellerProvisioningOutcome.Conflict)
            seller.MarkConflict(reason);
        else
            seller.MarkFailed(reason);

        bundle.AddValidationIssue(
            "Seller",
            seller.ExternalSellerId,
            "sellerOwnership",
            ImportValidationSeverity.Blocking,
            reason,
            $"Imported seller '{seller.DisplayName}' could not be provisioned: {reason}.");

        foreach (var product in bundle.Products.Where(product =>
            string.Equals(product.ExternalSellerId, seller.ExternalSellerId, StringComparison.OrdinalIgnoreCase)))
        {
            bundle.AddValidationIssue(
                "Product",
                product.ExternalProductId,
                "sellerOwnership",
                ImportValidationSeverity.Blocking,
                reason,
                $"Imported product '{product.Title}' is blocked because seller '{seller.DisplayName}' could not be provisioned: {reason}.");
        }
    }

    private static bool CanImport(CatalogImportBundle bundle, ImportedProduct product)
    {
        if (product.ImportStatus != ImportedProductImportStatus.NotImported)
            return false;
        if (product.ReadinessStatus is ImportedProductReadinessStatus.Blocked or ImportedProductReadinessStatus.Duplicate or ImportedProductReadinessStatus.Imported)
            return false;

        var seller = bundle.Sellers.FirstOrDefault(x =>
            string.Equals(x.ExternalSellerId, product.ExternalSellerId, StringComparison.OrdinalIgnoreCase));
        if (seller?.HiveSpaceStoreId is null)
            return false;

        if (product.ExternalCategoryIds.Count == 0
            || product.ExternalCategoryIds.Any(categoryId =>
                !bundle.CategoryMappings.Any(mapping =>
                    string.Equals(mapping.ExternalCategoryId, categoryId, StringComparison.OrdinalIgnoreCase)
                    && mapping.HiveSpaceCategoryId.HasValue)))
            return false;

        if (!product.Skus.Any(sku =>
                sku.PriceAmount is > 0
                && string.Equals(sku.CurrencyCode, "VND", StringComparison.OrdinalIgnoreCase)))
            return false;

        if (bundle.ValidationIssues.Any(issue =>
                issue.Severity == ImportValidationSeverity.Blocking
                && (string.Equals(issue.EntitySourceId, product.ExternalProductId, StringComparison.OrdinalIgnoreCase)
                    || product.Skus.Any(sku => string.Equals(issue.EntitySourceId, sku.ExternalSkuId, StringComparison.OrdinalIgnoreCase)))))
            return false;

        return bundle.DuplicateGroups.All(group =>
            !group.MemberImportedProductIds.Contains(product.Id)
            || group.RepresentativeImportedProductId == product.Id);
    }

    private static Product CreateProduct(
        CatalogImportBundle bundle,
        ImportedProduct importedProduct,
        Guid importedByUserId,
        string publicationState)
    {
        var seller = bundle.Sellers.Single(x =>
            string.Equals(x.ExternalSellerId, importedProduct.ExternalSellerId, StringComparison.OrdinalIgnoreCase));
        var categoryId = bundle.CategoryMappings
            .Where(mapping => importedProduct.ExternalCategoryIds.Contains(mapping.ExternalCategoryId, StringComparer.OrdinalIgnoreCase))
            .Select(mapping => mapping.HiveSpaceCategoryId)
            .First(id => id.HasValue)!.Value;
        var productImages = CreateProductImages(importedProduct);
        var skus = importedProduct.Skus
            .Where(sku => sku.PriceAmount is > 0 && string.Equals(sku.CurrencyCode, "VND", StringComparison.OrdinalIgnoreCase))
            .Select(sku => new Sku(
                sku.SkuNumber ?? sku.ExternalSkuId,
                CreateSkuVariants(sku.VariantSelectionsJson),
                CreateSkuImages(sku.ImageUrlsJson),
                sku.StockQuantity ?? 0,
                sku.IsActiveCandidate,
                Money.FromSmallestUnit(sku.PriceAmount!.Value, "VND")))
            .ToList();

        var product = Product.CreateProduct(
            name: importedProduct.Title,
            slug: $"{importedProduct.Title}-{Guid.NewGuid().ToString("N")[..6]}",
            description: importedProduct.Description ?? string.Empty,
            shortDescription: CreateShortDescription(importedProduct),
            status: ResolveImportedProductStatus(publicationState),
            storeId: seller.HiveSpaceStoreId!.Value,
            condition: ProductCondition.New,
            featured: false,
            categories: [new ProductCategory(categoryId)],
            attributes: CreateProductAttributes(importedProduct),
            images: productImages,
            skus: skus,
            variants: [],
            createdAt: DateTimeOffset.UtcNow,
            createdBy: importedByUserId.ToString());

        var thumbnailUrl = Normalize(importedProduct.ThumbnailImageExternalUrl);
        if (thumbnailUrl is not null)
            product.SetThumbnailUrl(thumbnailUrl);

        return product;
    }

    private static ProductStatus ResolveImportedProductStatus(string publicationState)
        => publicationState.Trim() switch
        {
            nameof(ProductStatus.Available) => ProductStatus.Available,
            nameof(ProductStatus.Draft) => ProductStatus.Draft,
            "Inactive" => ProductStatus.Unpublish,
            nameof(ProductStatus.Unpublish) => ProductStatus.Unpublish,
            _ => throw new InvalidFieldException(CatalogDomainErrorCode.InvalidProductStatus, nameof(publicationState))
        };

    private static List<SkuVariant> CreateSkuVariants(string variantSelectionsJson)
    {
        var selections = JsonSerializer.Deserialize<Dictionary<string, string>>(variantSelectionsJson) ?? [];
        return selections.Select(x => new SkuVariant(x.Key, x.Value)).ToList();
    }

    private static string? CreateShortDescription(ImportedProduct importedProduct)
    {
        var source = Normalize(importedProduct.Description) ?? Normalize(importedProduct.Title);
        if (source is null)
            return null;

        const int maxLength = 500;
        return source.Length <= maxLength ? source : source[..maxLength];
    }

    private static List<ProductImage> CreateProductImages(ImportedProduct importedProduct)
    {
        var images = importedProduct.Images
            .Where(image => image.ImportedSkuId is null)
            .Select(image => new ProductImage(0, image.SourceImageId ?? image.ExternalUrl, image.ExternalUrl))
            .ToList();

        var thumbnailUrl = Normalize(importedProduct.ThumbnailImageExternalUrl);
        if (thumbnailUrl is not null && !images.Any(image => string.Equals(image.ImageUrl, thumbnailUrl, StringComparison.OrdinalIgnoreCase)))
            images.Insert(0, new ProductImage(0, thumbnailUrl, thumbnailUrl));

        return images;
    }

    private static List<SkuImage> CreateSkuImages(string? imageUrlsJson)
    {
        var imageUrls = JsonSerializer.Deserialize<IReadOnlyCollection<string>>(imageUrlsJson ?? "[]") ?? [];
        return imageUrls
            .Select(Normalize)
            .Where(url => url is not null)
            .Select(url => new SkuImage(url!, url))
            .ToList();
    }

    private static List<ProductAttribute> CreateProductAttributes(ImportedProduct importedProduct)
        => importedProduct.Attributes
            .Where(attribute =>
                attribute.HiveSpaceAttributeDefinitionId.HasValue
                && (attribute.MatchedAttributeValueIds.Count > 0 || !string.IsNullOrWhiteSpace(attribute.ExternalAttributeValue)))
            .Select(attribute => new ProductAttribute(
                attribute.HiveSpaceAttributeDefinitionId!.Value,
                attribute.MatchedAttributeValueIds.Count > 0 ? attribute.MatchedAttributeValueIds : null,
                attribute.MatchedAttributeValueIds.Count == 0 ? attribute.ExternalAttributeValue : null))
            .ToList();

    private static void AddSellers(CatalogImportBundle bundle, IReadOnlyCollection<ImportedSellerRequestDto> sellers)
    {
        foreach (var seller in sellers)
        {
            bundle.AddSeller(
                seller.ExternalSellerId,
                seller.DisplayName,
                seller.Slug,
                seller.Url,
                seller.Metadata is null ? null : JsonSerializer.Serialize(seller.Metadata),
                seller.LogoUrl);
        }
    }

    private static void AddProducts(CatalogImportBundle bundle, IReadOnlyCollection<ImportedProductRequestDto> products)
    {
        foreach (var productDto in products)
        {
            var product = bundle.AddProduct(
                productDto.ExternalProductId,
                productDto.ExternalSellerId,
                productDto.Title,
                productDto.ExternalCategoryIds,
                productDto.Url,
                productDto.Description,
                productDto.ThumbnailUrl);

            foreach (var sku in productDto.Skus)
            {
                product.AddSku(
                    sku.ExternalSkuId,
                    sku.SkuNumber,
                    JsonSerializer.Serialize(sku.VariantSelections),
                    sku.Price.Amount,
                    sku.Price.SourceRawValue,
                    sku.Price.CurrencyCode,
                    JsonSerializer.Serialize(sku.ImageUrls),
                    sku.StockQuantity,
                    true);
            }

            foreach (var attribute in productDto.Attributes)
                product.AddAttribute(attribute.Name, attribute.Value, attribute.SourceAttributeId, attribute.SourceValueId);

            foreach (var image in productDto.Images)
                product.AddImage(image.Url, image.Role, image.SourceImageId);
        }
    }

    private static void AddDuplicateGroups(CatalogImportBundle bundle)
    {
        foreach (var group in bundle.Products
                     .GroupBy(product => product.ExternalProductId, StringComparer.OrdinalIgnoreCase)
                     .Where(group => group.Count() > 1))
        {
            var duplicateProducts = group.ToList();
            bundle.AddDuplicateGroup(
                $"{bundle.SourceSystem}:{group.Key}",
                duplicateProducts.Select(product => product.ExternalProductId).ToList(),
                duplicateProducts.Select(product => product.Id).ToList());
        }
    }

    private static void AddValidationHints(CatalogImportBundle bundle, IReadOnlyCollection<ImportValidationHintRequestDto> hints)
    {
        foreach (var hint in hints)
        {
            var severity = string.Equals(hint.Severity, nameof(ImportValidationSeverity.Blocking), StringComparison.OrdinalIgnoreCase)
                ? ImportValidationSeverity.Blocking
                : ImportValidationSeverity.Warning;

            bundle.AddValidationIssue(
                hint.EntityType,
                hint.EntitySourceId,
                hint.Field,
                severity,
                hint.ReasonCode,
                hint.Message);
        }
    }

    private static int? ParseProductSetId(string? productSetId)
        => int.TryParse(productSetId, out var value) ? value : null;

    private static AttributeType CreateAttributeType(
        string inputType,
        bool isRequired,
        bool hasSelectableValues)
    {
        var normalized = Normalize(inputType)?.ToLowerInvariant() ?? "textbox";
        var mappedInputType = normalized switch
        {
            "dropdown" => InputType.Dropdown,
            "radio" => InputType.Radio,
            "checkbox" => InputType.Checkbox,
            "combobox" => InputType.ComboBox,
            _ => InputType.Textbox
        };

        var valueType = mappedInputType switch
        {
            InputType.Checkbox
                => AttributeValueType.MultiSelect,
            InputType.Dropdown or
            InputType.Radio or
            InputType.ComboBox
                => AttributeValueType.SingleSelect,
            _ => hasSelectableValues
                ? AttributeValueType.SingleSelect
                : AttributeValueType.String
        };

        var maxValueCount = valueType == AttributeValueType.MultiSelect ? 10 : 1;
        return new AttributeType(valueType, mappedInputType, isRequired, maxValueCount);
    }

    private static void ApplyCategoryImage(Category existingCategory, CategoryProvisioningCategoryDto category)
    {
        var imageFileId = Normalize(category.ImageFileId);
        if (imageFileId is not null)
            existingCategory.SetImageFileId(imageFileId);

        SetImageUrl(existingCategory, category.ImageUrl);
    }

    private static void SetImageUrl(Category category, string? imageUrl)
    {
        var normalizedImageUrl = Normalize(imageUrl);
        if (normalizedImageUrl is not null)
            category.SetImageUrl(normalizedImageUrl);
    }

    private static T Deserialize<T>(string? payloadJson, string source)
    {
        if (string.IsNullOrWhiteSpace(payloadJson))
            throw new InvalidFieldException(CatalogDomainErrorCode.InvalidCatalogImportJob, source);

        return JsonSerializer.Deserialize<T>(payloadJson)
            ?? throw new InvalidFieldException(CatalogDomainErrorCode.InvalidCatalogImportJob, source);
    }

    private static string CreateResultSummary<T>(T result)
        => JsonSerializer.Serialize(result);

    private static T Required<T>(T? value, string field)
        where T : class
        => value ?? throw new InvalidFieldException(CatalogDomainErrorCode.InvalidCatalogImportJob, field);

    private static string? Normalize(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
