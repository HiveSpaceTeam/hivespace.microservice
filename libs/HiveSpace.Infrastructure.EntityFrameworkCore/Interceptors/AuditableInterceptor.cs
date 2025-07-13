using HiveSpace.Domain.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace HiveSpace.Infrastructure.EntityFrameworkCore.Interceptors;

/// <summary>
/// Interceptor for applying audit logic to entities implementing IAuditable.
/// </summary>
public class AuditableInterceptor : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        ApplyAudit(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        ApplyAudit(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private static void ApplyAudit(DbContext? context)
    {
        if (context == null) return;
        var now = DateTimeOffset.UtcNow;
        var auditableEntries = context.ChangeTracker.Entries<IAuditable>()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified)
            .ToList();
        foreach (var entry in auditableEntries)
        {
            if (entry.State == EntityState.Added)
            {
                var createdAtProp = entry.Property(nameof(IAuditable.CreatedAt));
                if (createdAtProp != null && (createdAtProp.CurrentValue == null || (DateTimeOffset)createdAtProp.CurrentValue == default))
                {
                    createdAtProp.CurrentValue = now;
                }
            }
            var updatedAtProp = entry.Property(nameof(IAuditable.UpdatedAt));
            if (updatedAtProp != null)
            {
                updatedAtProp.CurrentValue = now;
            }
        }
    }
} 