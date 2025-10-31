using HiveSpace.CatalogService.Domain.Aggregates.AttributeAggregate;

namespace HiveSpace.CatalogService.Application.Interfaces.Repositories
{
    public interface IAttributeRepository
    {
        Task<AttributeDefinition?> GetByIdAsync(Guid id);
        Task<List<AttributeDefinition>> GetAllAsync();
        Task AddAsync(AttributeDefinition attribute);
        Task UpdateAsync(AttributeDefinition attribute);
        Task DeleteAsync(AttributeDefinition attribute);
    }
}


