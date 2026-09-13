using HiveSpace.CatalogService.Domain.Aggregates.AttributeAggregate;
using HiveSpace.CatalogService.Domain.Repositories;
using HiveSpace.CatalogService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HiveSpace.CatalogService.Infrastructure.Repositories;

public class SqlAttributeRepository(CatalogDbContext context) : IAttributeRepository
{
    public async Task<AttributeDefinition?> GetByIdAsync(Guid id)
        => await context.Attributes.FindAsync(id);

    public async Task<List<AttributeDefinition>> GetAllAsync()
        => await context.Attributes
            .Include(x => x.Values)
            .ToListAsync();

    public async Task<List<AttributeDefinition>> GetByNamesAsync(IReadOnlyCollection<string> names)
    {
        var normalizedNames = names
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Select(name => name.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (normalizedNames.Count == 0)
            return [];

        return await context.Attributes
            .Include(x => x.Values)
            .Where(x => normalizedNames.Contains(x.Name))
            .ToListAsync();
    }

    public async Task AddAsync(AttributeDefinition attribute)
        => await context.Attributes.AddAsync(attribute);

    public Task UpdateAsync(AttributeDefinition attribute)
    {
        context.Attributes.Update(attribute);
        return Task.CompletedTask;
    }

    public void Remove(AttributeDefinition attribute)
        => context.Attributes.Remove(attribute);

    public async Task<int> SaveChangesAsync()
        => await context.SaveChangesAsync();
}
