using FluentAssertions;
using HiveSpace.CatalogService.Application.CatalogImports.Commands.ImportReadyProducts;
using HiveSpace.CatalogService.Application.CatalogImports.Jobs;
using HiveSpace.CatalogService.Application.Interfaces.Messaging;
using HiveSpace.CatalogService.Domain.Aggregates.ProductAggregate;
using HiveSpace.CatalogService.Domain.CatalogImports;
using HiveSpace.CatalogService.Domain.Repositories;
using HiveSpace.Domain.Shared.Enumerations;
using HiveSpace.Infrastructure.Messaging.Shared.Events.Products;
using HiveSpace.Testing.Shared.Doubles;
using System.Reflection;
using System.Text.Json;
using Xunit;

namespace HiveSpace.CatalogService.Tests.Application.CatalogImports;

public class ImportReadyProductsCommandHandlerTests
{
    private const string SellerLogoUrl = "https://cdn.example.com/sellers/tiki-trading.png";

    [Fact]
    public async Task Handle_WithReadyProducts_CreatesDraftInactiveSellerOwnedProducts()
    {
        var bundle = ValidateCatalogImportBundleCommandHandlerTests.CreateBundle(provisionCategory: true, priceAmount: 125000);
        bundle.RefreshSummary();
        var productRepository = new ProductRepositoryFake();
        var productEventPublisher = new RecordingProductEventPublisher();

        var job = await ExecuteImportAsync(bundle, productRepository, productEventPublisher);

        job.CreatedCount.Should().Be(1);
        productRepository.Products.Should().ContainSingle();
        productRepository.Products.Single().StoreId.Should().Be(bundle.Sellers.Single().HiveSpaceStoreId!.Value);
        productRepository.Products.Single().Status.Should().Be(ProductStatus.Draft);
        bundle.Products.Single().ImportStatus.ToString().Should().Be("Imported");
        productEventPublisher.ImportedReplicaSyncs.Should().ContainSingle();
    }

    [Fact]
    public async Task Handle_WithUnpublishPublicationState_CreatesUnpublishedSellerOwnedProduct()
    {
        var bundle = ValidateCatalogImportBundleCommandHandlerTests.CreateBundle(provisionCategory: true, priceAmount: 125000);
        bundle.RefreshSummary();
        var repository = new ValidateCatalogImportBundleCommandHandlerTests.CatalogImportBundleRepositoryFake(bundle);
        var productRepository = new ProductRepositoryFake();

        var submission = await new ImportReadyProductsCommandHandler(
            repository,
            new NullCatalogImportJobLifecyclePublisher())
            .Handle(new ImportReadyProductsCommand(bundle.Id, [bundle.Products.First().Id], nameof(ProductStatus.Unpublish), Guid.NewGuid()), CancellationToken.None);

        await new CatalogImportJobProcessor(
            repository,
            new ValidateCatalogImportBundleCommandHandlerTests.CategoryRepositoryFake(),
            new NullCatalogImportJobLifecyclePublisher(),
            productRepository: productRepository,
            productEventPublisher: new RecordingProductEventPublisher())
            .ProcessJobAsync(submission.JobId, CancellationToken.None);

        productRepository.Products.Should().ContainSingle();
        productRepository.Products.Single().Status.Should().Be(ProductStatus.Unpublish);
    }

    [Fact]
    public async Task Handle_WithExplicitDraftPublicationState_CreatesDraftSellerOwnedProduct()
    {
        var bundle = ValidateCatalogImportBundleCommandHandlerTests.CreateBundle(provisionCategory: true, priceAmount: 125000);
        bundle.RefreshSummary();
        var repository = new ValidateCatalogImportBundleCommandHandlerTests.CatalogImportBundleRepositoryFake(bundle);
        var productRepository = new ProductRepositoryFake();

        var submission = await new ImportReadyProductsCommandHandler(
            repository,
            new NullCatalogImportJobLifecyclePublisher())
            .Handle(new ImportReadyProductsCommand(bundle.Id, [bundle.Products.First().Id], nameof(ProductStatus.Draft), Guid.NewGuid()), CancellationToken.None);

        await new CatalogImportJobProcessor(
            repository,
            new ValidateCatalogImportBundleCommandHandlerTests.CategoryRepositoryFake(),
            new NullCatalogImportJobLifecyclePublisher(),
            productRepository: productRepository,
            productEventPublisher: new RecordingProductEventPublisher())
            .ProcessJobAsync(submission.JobId, CancellationToken.None);

        productRepository.Products.Should().ContainSingle();
        productRepository.Products.Single().Status.Should().Be(ProductStatus.Draft);
    }

    [Fact]
    public async Task Handle_WithAvailablePublicationState_CreatesAvailableSellerOwnedProduct()
    {
        var bundle = ValidateCatalogImportBundleCommandHandlerTests.CreateBundle(provisionCategory: true, priceAmount: 125000);
        bundle.RefreshSummary();
        var repository = new ValidateCatalogImportBundleCommandHandlerTests.CatalogImportBundleRepositoryFake(bundle);
        var productRepository = new ProductRepositoryFake();

        var submission = await new ImportReadyProductsCommandHandler(
            repository,
            new NullCatalogImportJobLifecyclePublisher())
            .Handle(new ImportReadyProductsCommand(bundle.Id, [bundle.Products.First().Id], nameof(ProductStatus.Available), Guid.NewGuid()), CancellationToken.None);

        await new CatalogImportJobProcessor(
            repository,
            new ValidateCatalogImportBundleCommandHandlerTests.CategoryRepositoryFake(),
            new NullCatalogImportJobLifecyclePublisher(),
            productRepository: productRepository,
            productEventPublisher: new RecordingProductEventPublisher())
            .ProcessJobAsync(submission.JobId, CancellationToken.None);

        productRepository.Products.Should().ContainSingle();
        productRepository.Products.Single().Status.Should().Be(ProductStatus.Available);
    }
    [Fact]
    public async Task Handle_WithBlockedProducts_SkipsAndReportsBlockedResults()
    {
        var bundle = ValidateCatalogImportBundleCommandHandlerTests.CreateBundle(provisionCategory: false, priceAmount: null);
        bundle.RefreshSummary();
        var productRepository = new ProductRepositoryFake();

        var job = await ExecuteImportAsync(bundle, productRepository, new RecordingProductEventPublisher());

        job.CreatedCount.Should().Be(0);
        job.BlockedCount.Should().Be(1);
        productRepository.Products.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WithDuplicateRepresentativeOnly_ImportsOneProduct()
    {
        var bundle = ValidateCatalogImportBundleCommandHandlerTests.CreateBundle(provisionCategory: true, priceAmount: 125000);
        var product = bundle.Products.Single();
        bundle.AddDuplicateGroup("tiki:product-1", ["product-1", "product-1-copy"], [product.Id, Guid.NewGuid()])
            .Resolve(product.Id);
        bundle.RefreshSummary();
        var productRepository = new ProductRepositoryFake();

        var job = await ExecuteImportAsync(bundle, productRepository, new RecordingProductEventPublisher());

        job.CreatedCount.Should().Be(1);
        productRepository.Products.Should().ContainSingle();
    }

    [Fact]
    public async Task Handle_WithImportedDescriptionsAndImages_MapsShortDescriptionAndImageUrls()
    {
        var bundle = CreateBundleWithMedia();
        bundle.RefreshSummary();
        var productRepository = new ProductRepositoryFake();

        var job = await ExecuteImportAsync(bundle, productRepository, new RecordingProductEventPublisher());

        job.CreatedCount.Should().Be(1);
        var product = productRepository.Products.Single();
        product.ShortDescription.Should().Be(bundle.Products.Single().Description);
        product.ThumbnailUrl.Should().Be("https://cdn.example.com/products/widget-thumb.png");
        product.Images.Should().ContainSingle(image => image.ImageUrl == "https://cdn.example.com/products/widget-thumb.png");
        product.Images.Should().ContainSingle(image => image.ImageUrl == "https://cdn.example.com/products/widget-side.png");
        product.Skus.Should().ContainSingle();
        product.Skus.Single().Images.Should().ContainSingle(image => image.ImageUrl == "https://cdn.example.com/skus/widget-blue.png");
    }

    [Fact]
    public async Task Handle_WithReadyProducts_PublishesBatchReplicaSyncUsingSavedIds()
    {
        var bundle = CreateBundleWithTwoReadyProducts();
        var productRepository = new ProductRepositoryFake();
        var productEventPublisher = new RecordingProductEventPublisher();
        var repository = new ValidateCatalogImportBundleCommandHandlerTests.CatalogImportBundleRepositoryFake(bundle);

        var submission = await new ImportReadyProductsCommandHandler(
            repository,
            new NullCatalogImportJobLifecyclePublisher())
            .Handle(new ImportReadyProductsCommand(bundle.Id, null, "Draft", Guid.NewGuid()), CancellationToken.None);

        await new CatalogImportJobProcessor(
            repository,
            new ValidateCatalogImportBundleCommandHandlerTests.CategoryRepositoryFake(),
            new NullCatalogImportJobLifecyclePublisher(),
            productRepository: productRepository,
            productEventPublisher: productEventPublisher)
            .ProcessJobAsync(submission.JobId, CancellationToken.None);

        productEventPublisher.Created.Should().BeEmpty();
        productEventPublisher.ImportedReplicaSyncs.Should().ContainSingle();
        var batch = productEventPublisher.ImportedReplicaSyncs.Single();
        batch.BundleId.Should().Be(bundle.Id);
        batch.Products.Should().HaveCount(2);
        batch.Products.Select(x => x.ProductId).Should().OnlyContain(id => id > 0);
        batch.Products.SelectMany(x => x.Skus).Select(x => x.SkuId).Should().OnlyContain(id => id > 0);
    }

    [Fact]
    public async Task Handle_WithoutProductIds_ReturnsAcceptedImportAllReadyJob()
    {
        var bundle = CreateBundleWithTwoReadyProducts();
        var repository = new ValidateCatalogImportBundleCommandHandlerTests.CatalogImportBundleRepositoryFake(bundle);

        var submission = await new ImportReadyProductsCommandHandler(
            repository,
            new NullCatalogImportJobLifecyclePublisher())
            .Handle(new ImportReadyProductsCommand(bundle.Id, null, "Draft", Guid.NewGuid()), CancellationToken.None);

        var job = await repository.GetJobByIdAsync(submission.JobId, CancellationToken.None);

        job.Should().NotBeNull();
        job!.RequestPayloadJson.Should().Contain(bundle.Products.First().Id.ToString());
        job.RequestPayloadJson.Should().Contain(bundle.Products.Last().Id.ToString());
    }

    [Fact]
    public async Task Handle_WithEmptyProductIds_ReturnsAcceptedImportAllReadyJob()
    {
        var bundle = CreateBundleWithTwoReadyProducts();
        var repository = new ValidateCatalogImportBundleCommandHandlerTests.CatalogImportBundleRepositoryFake(bundle);

        var submission = await new ImportReadyProductsCommandHandler(
            repository,
            new NullCatalogImportJobLifecyclePublisher())
            .Handle(new ImportReadyProductsCommand(bundle.Id, [], "Draft", Guid.NewGuid()), CancellationToken.None);

        var job = await repository.GetJobByIdAsync(submission.JobId, CancellationToken.None);

        job.Should().NotBeNull();
        job!.RequestPayloadJson.Should().Contain(bundle.Products.First().Id.ToString());
        job.RequestPayloadJson.Should().Contain(bundle.Products.Last().Id.ToString());
    }

    [Fact]
    public async Task Worker_WithImportAllReadyRequest_UsesSubmitTimeSnapshot()
    {
        var bundle = CreateBundleWithTwoProducts(secondProductReady: false);
        var repository = new ValidateCatalogImportBundleCommandHandlerTests.CatalogImportBundleRepositoryFake(bundle);
        var productRepository = new ProductRepositoryFake();

        var submission = await new ImportReadyProductsCommandHandler(
            repository,
            new NullCatalogImportJobLifecyclePublisher())
            .Handle(new ImportReadyProductsCommand(bundle.Id, null, "Draft", Guid.NewGuid()), CancellationToken.None);

        bundle.AddCategoryLink("9999", null, "Late mapped category", 2);
        bundle.ReplaceValidationIssues([]);
        bundle.RefreshSummary();

        await new CatalogImportJobProcessor(
            repository,
            new ValidateCatalogImportBundleCommandHandlerTests.CategoryRepositoryFake(),
            new NullCatalogImportJobLifecyclePublisher(),
            productRepository: productRepository,
            productEventPublisher: new RecordingProductEventPublisher())
            .ProcessJobAsync(submission.JobId, CancellationToken.None);

        productRepository.Products.Should().ContainSingle();
        productRepository.Products.Single().Name.Should().Be(bundle.Products.First().Title);
    }

    private static async Task<CatalogImportJob> ExecuteImportAsync(
        CatalogImportBundle bundle,
        ProductRepositoryFake productRepository,
        RecordingProductEventPublisher productEventPublisher)
    {
        var repository = new ValidateCatalogImportBundleCommandHandlerTests.CatalogImportBundleRepositoryFake(bundle);
        var submission = await new ImportReadyProductsCommandHandler(
            repository,
            new NullCatalogImportJobLifecyclePublisher())
            .Handle(new ImportReadyProductsCommand(bundle.Id, [bundle.Products.First().Id], "Draft", Guid.NewGuid()), CancellationToken.None);

        await new CatalogImportJobProcessor(
            repository,
            new ValidateCatalogImportBundleCommandHandlerTests.CategoryRepositoryFake(),
            new NullCatalogImportJobLifecyclePublisher(),
            productRepository: productRepository,
            productEventPublisher: productEventPublisher)
            .ProcessJobAsync(submission.JobId, CancellationToken.None);

        return (await repository.GetJobByIdAsync(submission.JobId, CancellationToken.None))!;
    }

    private static CatalogImportBundle CreateBundleWithTwoReadyProducts()
    {
        var bundle = CreateBundleWithTwoProducts(secondProductReady: true);
        bundle.RefreshSummary();
        return bundle;
    }

    private static CatalogImportBundle CreateBundleWithTwoProducts(bool secondProductReady)
    {
        var bundle = ValidateCatalogImportBundleCommandHandlerTests.CreateBundle(provisionCategory: true, priceAmount: 125000);
        var secondProduct = bundle.AddProduct("product-2", "seller-1", "Notebook", [secondProductReady ? "1846" : "9999"], null, "Description", null);
        secondProduct.AddSku("sku-2", "TIKI-SKU-2", "{}", 99000, "99000", "VND", null, 5, true);
        bundle.ReplaceValidationIssues([]);
        return bundle;
    }

    private static CatalogImportBundle CreateBundleWithMedia()
    {
        var bundle = CatalogImportBundle.Create(
            "2026-07-24",
            "tiki",
            "category",
            "1846",
            $"sha256:{Guid.NewGuid():N}",
            DateTimeOffset.UtcNow,
            Guid.NewGuid());

        bundle.AddSeller("seller-1", "Tiki Trading", null, null, null, SellerLogoUrl)
            .MarkProvisioned(Guid.NewGuid(), Guid.NewGuid(), created: false);
        bundle.AddCategoryLink("1846", null, "Nha sach Tiki", 1);

        var product = bundle.AddProduct(
            "product-1",
            "seller-1",
            "Widget",
            ["1846"],
            "https://tiki.vn/widget",
            "Imported widget short description",
            "https://cdn.example.com/products/widget-thumb.png");

        product.AddImage("https://cdn.example.com/products/widget-thumb.png", "thumbnail", "thumb-1");
        product.AddImage("https://cdn.example.com/products/widget-side.png", "gallery", "gallery-1");
        product.AddSku(
            "sku-1",
            "TIKI-SKU-1",
            "{}",
            125000,
            "125000",
            "VND",
            JsonSerializer.Serialize(new[] { "https://cdn.example.com/skus/widget-blue.png" }),
            10,
            true);

        bundle.ReplaceValidationIssues([]);
        return bundle;
    }

    private sealed class ProductRepositoryFake : IProductRepository
    {
        public List<Product> Products { get; } = [];

        public Task<Product?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
            => Task.FromResult<Product?>(Products.FirstOrDefault(x => x.Id == id));

        public Task<List<Product>> GetAllAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(Products);

        public Task<Product?> GetDetailByIdAsync(int id, bool noTracking, CancellationToken cancellationToken = default)
            => Task.FromResult<Product?>(Products.FirstOrDefault(x => x.Id == id));

        public Task AddAsync(Product product, CancellationToken cancellationToken = default)
        {
            typeof(Product)
                .BaseType!
                .GetProperty("Id", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!
                .SetValue(product, Products.Count + 1);
            var nextSkuId = Products.SelectMany(existing => existing.Skus).Count() + 1;
            foreach (var sku in product.Skus)
            {
                typeof(Sku)
                    .BaseType!
                    .GetProperty("Id", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!
                    .SetValue(sku, nextSkuId++);
            }
            Products.Add(product);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(Product product, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public void Remove(Product product)
        {
            Products.Remove(product);
        }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(1);

        public Task<(IReadOnlyList<Product> Items, int Total)> GetPagedAsync(string keyword, int pageIndex, int pageSize, string sort, Guid sellerId, CancellationToken cancellationToken = default)
            => Task.FromResult<(IReadOnlyList<Product> Items, int Total)>((Products, Products.Count));

        public Task<(IReadOnlyList<Product> Items, int Total)> GetSummariesPagedAsync(string keyword, int pageIndex, int pageSize, string sort, CancellationToken cancellationToken = default)
            => Task.FromResult<(IReadOnlyList<Product> Items, int Total)>((Products, Products.Count));

        public Task<IReadOnlyList<Product>> FindSimilarByTitleAsync(string title, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<Product>>(Products.Where(x => string.Equals(x.Name, title, StringComparison.OrdinalIgnoreCase)).ToList());
    }

    private sealed class RecordingProductEventPublisher : IProductEventPublisher
    {
        public List<Product> Created { get; } = [];
        public List<ImportedProductsReplicaSyncIntegrationEvent> ImportedReplicaSyncs { get; } = [];

        public Task PublishProductCreatedAsync(Product product, CancellationToken cancellationToken = default)
        {
            Created.Add(product);
            return Task.CompletedTask;
        }

        public Task PublishProductUpdatedAsync(Product product, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task PublishProductDeletedAsync(Product product, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task PublishSkuUpdatedAsync(Product product, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task PublishImportedProductsReplicaSyncAsync(
            CatalogImportBundle bundle,
            IReadOnlyCollection<Product> products,
            CancellationToken cancellationToken = default)
        {
            ImportedReplicaSyncs.Add(new ImportedProductsReplicaSyncIntegrationEvent(
                bundle.Id,
                bundle.SourceSystem,
                bundle.SourceFingerprint,
                DateTimeOffset.UtcNow,
                products.Select(product => new ImportedProductReplicaSyncItem(
                    product.Id,
        product.StoreId,
                    product.Name,
                    product.ThumbnailUrl,
                    product.Status,
                    product.Skus.Select(sku => new ImportedSkuReplicaSyncItem(
                        sku.Id,
                        product.Id,
                        sku.SkuNo,
                        string.Join(", ", sku.SkuVariants.Select(variant => variant.Value)),
                        sku.Price.Amount,
                        sku.Price.Currency.ToString(),
                        sku.Images.FirstOrDefault()?.ImageUrl))
                    .ToList()))
                .ToList()));
            return Task.CompletedTask;
        }
    }
}
