using HiveSpace.CatalogService.Domain.Aggregates.AttributeAggregate;
using HiveSpace.CatalogService.Domain.Aggregates.CategoryAggregate;
using HiveSpace.CatalogService.Domain.Aggregates.External;
using HiveSpace.CatalogService.Domain.Aggregates.ProductAggregate;
using HiveSpace.CatalogService.Domain.CatalogImports;
using HiveSpace.CatalogService.Infrastructure.EntityConfigurations;
using HiveSpace.Infrastructure.Messaging.Extensions;
using HiveSpace.Infrastructure.Persistence;
using HiveSpace.Infrastructure.Persistence.Idempotence;
using Microsoft.EntityFrameworkCore;

namespace HiveSpace.CatalogService.Infrastructure.Data
{
    public class CatalogDbContext : DbContext
    {
        public CatalogDbContext(DbContextOptions<CatalogDbContext> options) : base(options) { }

        #region Domain
        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<AttributeDefinition> Attributes { get; set; }
        public DbSet<Sku> Skus { get; set; }
        public DbSet<ProductAttribute> ProductAttributes { get; set; }
        public DbSet<AttributeValue> AttributeValues { get; set; }
        public DbSet<CatalogImportBundle> CatalogImportBundles { get; set; }
        public DbSet<CatalogImportJob> CatalogImportJobs { get; set; }
        public DbSet<ExternalCategoryLink> ExternalCategoryLinks { get; set; }
        public DbSet<ExternalCategoryAttributeLink> ExternalCategoryAttributeLinks { get; set; }

        #endregion

        #region ReadModels
        public DbSet<StoreRef> StoreRef { get; set; }
        public DbSet<PlatformCurrencyPolicyRef> PlatformCurrencyPolicyRefs { get; set; }
        #endregion

        public DbSet<IncomingRequest> IncomingRequest { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Apply all entity configurations
            modelBuilder.ApplyConfiguration(new ProductConfiguration());
            modelBuilder.ApplyConfiguration(new CategoryConfiguration());
            modelBuilder.ApplyConfiguration(new AttributeConfiguration());
            modelBuilder.ApplyConfiguration(new SkuConfiguration());
            modelBuilder.ApplyConfiguration(new ProductVariantConfiguration());
            modelBuilder.ApplyConfiguration(new AttributeValueConfiguration());
            modelBuilder.ApplyConfiguration(new CatalogImportBundleConfiguration());
            modelBuilder.ApplyConfiguration(new CatalogImportJobConfiguration());
            modelBuilder.ApplyConfiguration(new ExternalCategoryLinkConfiguration());
            modelBuilder.ApplyConfiguration(new ExternalCategoryAttributeLinkConfiguration());
            modelBuilder.ApplyConfiguration(new ImportedSellerConfiguration());
            modelBuilder.ApplyConfiguration(new ImportedCategoryMappingConfiguration());
            modelBuilder.ApplyConfiguration(new ImportedProductConfiguration());
            modelBuilder.ApplyConfiguration(new ImportedSkuConfiguration());
            modelBuilder.ApplyConfiguration(new ImportedAttributeConfiguration());
            modelBuilder.ApplyConfiguration(new ImportedImageReferenceConfiguration());
            modelBuilder.ApplyConfiguration(new ImportValidationIssueConfiguration());
            modelBuilder.ApplyConfiguration(new ImportDuplicateGroupConfiguration());
            modelBuilder.ApplyConfiguration(new SellerOwnershipLinkConfiguration());

            modelBuilder.ApplyConfiguration(new StoreRefValueConfiguration());
            modelBuilder.ApplyConfiguration(new PlatformCurrencyPolicyRefConfiguration());
            modelBuilder.AddPersistenceBuilder();
            modelBuilder.AddEntityOutBox();
        }
    }
}
