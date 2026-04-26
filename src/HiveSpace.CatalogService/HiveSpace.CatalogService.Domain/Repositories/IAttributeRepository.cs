using HiveSpace.CatalogService.Domain.Aggregates.AttributeAggregate;

namespace HiveSpace.CatalogService.Domain.Repositories
{
    public interface IAttributeRepository
    {
        Task<AttributeDefinition?> GetByIdAsync(Guid id);
        Task<List<AttributeDefinition>> GetAllAsync();
        Task AddAsync(AttributeDefinition attribute);
        Task UpdateAsync(AttributeDefinition attribute);
        void Remove(AttributeDefinition attribute);
        Task<int> SaveChangesAsync();
    }
}


