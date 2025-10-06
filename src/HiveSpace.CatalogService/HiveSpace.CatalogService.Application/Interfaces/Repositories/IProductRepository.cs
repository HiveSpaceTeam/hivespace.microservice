using HiveSpace.CatalogService.Domain.Aggregates.ProductAggregate;

namespace HiveSpace.CatalogService.Application.Interfaces.Repositories
{
    public interface IProductRepository
    {
        Task<Product?> GetByIdAsync(Guid id);
        Task<List<Product>> GetAllAsync();
        Task<Product?> GetDetailByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task AddAsync(Product product, CancellationToken cancellationToken = default);
        Task UpdateAsync(Product product, CancellationToken cancellationToken = default);
        Task DeleteAsync(Product product, CancellationToken cancellationToken = default);
        Task<(IReadOnlyList<Product> Items, int Total)> GetPagedAsync(string keyword, int pageIndex, int pageSize, string sort, CancellationToken cancellationToken = default);
    }
}


