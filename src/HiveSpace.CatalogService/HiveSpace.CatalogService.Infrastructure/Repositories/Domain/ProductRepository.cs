using HiveSpace.CatalogService.Application.Interfaces.Repositories.Domain;
using HiveSpace.CatalogService.Domain.Aggregates.ProductAggregate;
using HiveSpace.CatalogService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HiveSpace.CatalogService.Infrastructure.Repositories.Domain
{
    public class ProductRepository : IProductRepository
    {
        private readonly CatalogDbContext _context;
        public ProductRepository(CatalogDbContext context)
        {
            _context = context;
        }

        public async Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Products.FindAsync(new object?[] { id }, cancellationToken);
        }

        public async Task<List<Product>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Products.ToListAsync(cancellationToken);
        }

        public async Task AddAsync(Product product, CancellationToken cancellationToken = default)
        {
            await _context.Products.AddAsync(product, cancellationToken);
        }

        public async Task UpdateAsync(Product product, CancellationToken cancellationToken = default)
        {
            _context.Products.Update(product);
        }

        public async Task DeleteAsync(Product product, CancellationToken cancellationToken = default)
        {
            _context.Products.Remove(product);
            // Don't call SaveChangesAsync here - let the transaction service handle it
        }

        public async Task<Product?> GetDetailByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Products
                .AsNoTracking()
                .Where(p => p.Id == id)
                .Include(p => p.Categories)
                .Include(p => p.Images)
                .Include(p => p.Attributes)
                .Include(p => p.Variants)
                    .ThenInclude(v => v.Options)
                .Include(p => p.Skus)
                    .ThenInclude(s => s.Images)
                .Include(p => p.Skus)
                    .ThenInclude(s => s.SkuVariants)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<(IReadOnlyList<Product> Items, int Total)> GetPagedAsync(string keyword, int pageIndex, int pageSize, string sort, CancellationToken cancellationToken = default)
        {
            if (pageIndex < 1) pageIndex = 1;
            if (pageSize <= 0) pageSize = 10;

            var baseQuery = _context.Products.AsQueryable();

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
    }
}
