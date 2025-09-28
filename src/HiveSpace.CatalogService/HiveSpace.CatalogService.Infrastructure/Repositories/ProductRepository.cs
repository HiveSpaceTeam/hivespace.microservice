using HiveSpace.CatalogService.Domain.Aggregates.ProductAggregate;
using HiveSpace.CatalogService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HiveSpace.CatalogService.Infrastructure.Repositories
{
    public class ProductRepository
    {
        private readonly CatalogDbContext _context;
        public ProductRepository(CatalogDbContext context)
        {
            _context = context;
        }

        public async Task<Product?> GetByIdAsync(Guid id)
        {
            return await _context.Products.FindAsync(id);
        }

        public async Task<List<Product>> GetAllAsync()
        {
            return await _context.Products.ToListAsync();
        }

        public async Task AddAsync(Product product)
        {
            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Product product)
        {
            _context.Products.Update(product);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Product product)
        {
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
        }
    }
}
