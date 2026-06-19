using HiveSpace.CatalogService.Infrastructure.Data;
using HiveSpace.Infrastructure.Persistence.Transaction;
using Microsoft.EntityFrameworkCore.Storage;

namespace HiveSpace.CatalogService.Tests.Fakes;

public class FakeCatalogTransactionService(CatalogDbContext context) : ITransactionService
{
    public async Task InTransactionScopeAsync(Func<IDbContextTransaction, Task> action, bool performIdempotenceCheck = false, string actionName = "")
    {
        await action(null!);
        await context.SaveChangesAsync();
    }

    public Task IdempotenceCheckAsync() => Task.CompletedTask;
    public Task OutOfOrderCheckAsync() => Task.CompletedTask;
}
