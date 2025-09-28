using HiveSpace.CatalogService.Domain.Aggregates.ProductAggregate;
using HiveSpace.CatalogService.Domain.Aggregates.CategoryAggregate;
using HiveSpace.CatalogService.Domain.Aggregates.AttributeAggregate;
using Microsoft.EntityFrameworkCore;

namespace HiveSpace.CatalogService.Infrastructure.Data
{
    public class CatalogDbContext : DbContext
    {
        public CatalogDbContext(DbContextOptions<CatalogDbContext> options) : base(options) { }

        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<AttributeDefinition> Attributes { get; set; }
        public DbSet<Sku> Skus { get; set; }
        public DbSet<ProductAttribute> ProductAttributes { get; set; }
        public DbSet<AttributeValue> AttributeValues { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Product>().ToTable("Products");
            modelBuilder.Entity<Category>().ToTable("Categories");
            modelBuilder.Entity<AttributeDefinition>().ToTable("Attributes");
            modelBuilder.Entity<Sku>().ToTable("Skus");
            modelBuilder.Entity<ProductAttribute>().ToTable("ProductAttributes");
            modelBuilder.Entity<AttributeValue>().ToTable("AttributeValues");
        }
    }
}
