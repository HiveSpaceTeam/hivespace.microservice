using HiveSpace.CatalogService.Domain.Aggregates.CategoryAggregate;
using HiveSpace.CatalogService.Domain.Repositories;
using HiveSpace.CatalogService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HiveSpace.CatalogService.Infrastructure.Repositories;

public class SqlCategoryRepository(CatalogDbContext context) : ICategoryRepository
{
    public async Task<Category?> GetByIdAsync(int id)
        => await context.Categories.FindAsync(id);

    public async Task<List<Category>> GetAllAsync()
        => await context.Categories.ToListAsync();

    public async Task AddAsync(Category category)
        => await context.Categories.AddAsync(category);

    public Task UpdateAsync(Category category)
    {
        context.Categories.Update(category);
        return Task.CompletedTask;
    }

    public void Remove(Category category)
        => context.Categories.Remove(category);

    public async Task<int> SaveChangesAsync()
        => await context.SaveChangesAsync();
}
