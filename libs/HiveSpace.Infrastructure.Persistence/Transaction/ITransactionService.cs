using System;
using Microsoft.EntityFrameworkCore.Storage;

namespace HiveSpace.Infrastructure.Persistence.Transaction;

public interface ITransactionService
{
    Task InTransactionScopeAsync(Func<IDbContextTransaction, Task> action, bool performIdempotenceCheck = false, string actionName = "");
    Task IdempotenceCheckAsync();
    Task OutOfOrderCheckAsync();
}

