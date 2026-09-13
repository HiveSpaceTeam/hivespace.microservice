using HiveSpace.CatalogService.Domain.Aggregates.AttributeAggregate;

namespace HiveSpace.CatalogService.Domain.Repositories
{
    public interface IAttributeRepository
    {
        Task<AttributeDefinition?> GetByIdAsync(Guid id);
        Task<List<AttributeDefinition>> GetAllAsync();
        Task<List<AttributeDefinition>> GetByNamesAsync(IReadOnlyCollection<string> names);
        Task AddAsync(AttributeDefinition attribute);
        Task UpdateAsync(AttributeDefinition attribute);
        void Remove(AttributeDefinition attribute);
        Task<int> SaveChangesAsync();
    }
}

