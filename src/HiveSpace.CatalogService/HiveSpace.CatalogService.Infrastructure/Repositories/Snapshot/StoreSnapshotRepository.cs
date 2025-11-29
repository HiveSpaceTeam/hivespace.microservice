using HiveSpace.CatalogService.Application.Interfaces.Repositories.Snapshot;
using HiveSpace.CatalogService.Application.Models.ReadModels;
using HiveSpace.CatalogService.Domain.Aggregates.ProductAggregate;
using HiveSpace.CatalogService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;

namespace HiveSpace.CatalogService.Infrastructure.Repositories.Domain
{
    public class StoreSnapshotRepository : IStoreSnapshotRepository
    {
        private readonly CatalogDbContext _context;
        public StoreSnapshotRepository(CatalogDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(StoreSnapshot store, CancellationToken cancellationToken = default)
        {
            await _context.StoreSnapshots.AddAsync(store, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken); // thêm dòng này
        }
    }
}
