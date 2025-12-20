using HiveSpace.CatalogService.Domain.Aggregates.ProductAggregate;
using HiveSpace.CatalogService.Domain.Aggregates.CategoryAggregate;
using HiveSpace.CatalogService.Domain.Aggregates.AttributeAggregate;
using HiveSpace.CatalogService.Infrastructure.Data.Configurations;
using HiveSpace.Infrastructure.Persistence;
using HiveSpace.Infrastructure.Persistence.Idempotence;
using HiveSpace.Infrastructure.Persistence.Outbox;
using Microsoft.EntityFrameworkCore;
using HiveSpace.CatalogService.Application.Models.ReadModels;
using MassTransit;

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
        public DbSet<IncomingRequest> IncomingRequests { get; set; }
        public DbSet<OutboxMessage> OutboxMessages { get; set; }
        #endregion

        #region ReadModels
        public DbSet<StoreSnapshot> StoreSnapshots { get; set; }
        #endregion

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Apply all entity configurations
            modelBuilder.ApplyConfiguration(new ProductConfiguration());
            modelBuilder.ApplyConfiguration(new CategoryConfiguration());
            modelBuilder.ApplyConfiguration(new AttributeConfiguration());
            modelBuilder.ApplyConfiguration(new SkuConfiguration());
            modelBuilder.ApplyConfiguration(new ProductAttributeConfiguration());
            modelBuilder.ApplyConfiguration(new ProductVariantConfiguration());
            modelBuilder.ApplyConfiguration(new AttributeValueConfiguration());

            modelBuilder.ApplyConfiguration(new StoreSnapshotValueConfiguration());
            modelBuilder.AddPersistenceBuilder();

            modelBuilder.AddInboxStateEntity();
            modelBuilder.AddOutboxMessageEntity();
            modelBuilder.AddOutboxStateEntity();
        }
    }
}
