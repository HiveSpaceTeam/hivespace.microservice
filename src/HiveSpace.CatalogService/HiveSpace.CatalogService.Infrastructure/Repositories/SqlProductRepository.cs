using HiveSpace.CatalogService.Domain.Aggregates.ProductAggregate;
using HiveSpace.CatalogService.Domain.Aggregates.ProductAggregate.Specifications;
using HiveSpace.CatalogService.Domain.Repositories;
using HiveSpace.CatalogService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HiveSpace.CatalogService.Infrastructure.Repositories
{
    public class SqlProductRepository(CatalogDbContext context) : IProductRepository
    {
        public async Task<Product?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await context.Products.FindAsync(new object?[] { id }, cancellationToken);
        }

        public async Task<List<Product>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await context.Products.ToListAsync(cancellationToken);
        }

        public async Task AddAsync(Product product, CancellationToken cancellationToken = default)
        {
            await context.Products.AddAsync(product, cancellationToken);
        }

        public async Task UpdateAsync(Product product, CancellationToken cancellationToken = default)
        {
            context.Products.Update(product);
        }

        public void Remove(Product product)
        {
            context.Products.Remove(product);
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await context.SaveChangesAsync(cancellationToken);
        }

        public async Task<Product?> GetDetailByIdAsync(int id, bool noTracking = true, CancellationToken cancellationToken = default)
        {
            IQueryable<Product> query = context.Products;

            if (noTracking)
            {
                query = query.AsNoTracking();
            }

            query = query
             .Where(p => p.Id == id)
                .Include(p => p.Categories)
                .Include(p => p.Images)
                .Include(p => p.Attributes)
                .Include(p => p.Variants)
                    .ThenInclude(v => v.Options)
                .Include(p => p.Skus)
                    .ThenInclude(s => s.Images)
                .Include(p => p.Skus)
                    .ThenInclude(s => s.SkuVariants);

            return await query.FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<(IReadOnlyList<Product> Items, int Total)> GetPagedAsync(string keyword, int pageIndex, int pageSize, string sort, Guid sellerId, CancellationToken cancellationToken = default)
        {
            var baseQuery = context.Products
                .Where(new ProductOwnedBySellerSpecification(sellerId));

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                baseQuery = baseQuery.Where(p => p.Name.Contains(keyword));
            }

            baseQuery = (sort?.ToUpperInvariant() == "DESC")
                ? baseQuery.OrderByDescending(p => p.CreatedAt)
                : baseQuery.OrderBy(p => p.CreatedAt);

            var total = await baseQuery.CountAsync(cancellationToken);

            var pagedQuery = baseQuery
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .Include(p => p.Categories)
                .Include(p => p.Images)
                .Include(p => p.Attributes)
                .Include(p => p.Variants)
                    .ThenInclude(v => v.Options)
                .Include(p => p.Skus)
                    .ThenInclude(s => s.Images)
                .Include(p => p.Skus)
                    .ThenInclude(s => s.SkuVariants);

            var items = await pagedQuery.ToListAsync(cancellationToken);
            return (items, total);
        }

        public async Task<(IReadOnlyList<Product> Items, int Total)> GetSummariesPagedAsync(string keyword, int pageIndex, int pageSize, string sort, CancellationToken cancellationToken = default)
        {
            var baseQuery = context.Products
                .Where(new ProductActiveSpecification());

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                baseQuery = baseQuery.Where(p => p.Name.Contains(keyword));
            }

            baseQuery = (sort?.ToUpperInvariant() == "DESC")
                ? baseQuery.OrderByDescending(p => p.CreatedAt)
                : baseQuery.OrderBy(p => p.CreatedAt);

            var total = await baseQuery.CountAsync(cancellationToken);

            var pagedQuery = baseQuery
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .Include(p => p.Categories)
                .Include(p => p.Skus)
                    .ThenInclude(s => s.Images);

            var items = await pagedQuery.ToListAsync(cancellationToken);
            return (items, total);
        }
    }
}
