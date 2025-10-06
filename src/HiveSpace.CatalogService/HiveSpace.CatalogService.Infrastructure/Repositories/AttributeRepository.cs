using HiveSpace.CatalogService.Domain.Aggregates.AttributeAggregate;
using HiveSpace.CatalogService.Infrastructure.Data;
using HiveSpace.CatalogService.Application.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace HiveSpace.CatalogService.Infrastructure.Repositories
{
    public class AttributeRepository : IAttributeRepository
    {
        private readonly CatalogDbContext _context;
        public AttributeRepository(CatalogDbContext context)
        {
            _context = context;
        }

        public async Task<AttributeDefinition?> GetByIdAsync(Guid id)
        {
            return await _context.Attributes.FindAsync(id);
        }

        public async Task<List<AttributeDefinition>> GetAllAsync()
        {
            return await _context.Attributes.ToListAsync();
        }

        public async Task AddAsync(AttributeDefinition attribute)
        {
            await _context.Attributes.AddAsync(attribute);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(AttributeDefinition attribute)
        {
            _context.Attributes.Update(attribute);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(AttributeDefinition attribute)
        {
            _context.Attributes.Remove(attribute);
            await _context.SaveChangesAsync();
        }
    }
}
