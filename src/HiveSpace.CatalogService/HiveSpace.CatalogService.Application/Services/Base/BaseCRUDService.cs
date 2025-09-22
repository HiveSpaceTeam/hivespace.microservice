using HiveSpace.CatalogService.Application.Interfaces.Base;
using HiveSpace.CatalogService.Application.Models.Dtos.Crud;
using HiveSpace.Domain.Shared.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HiveSpace.CatalogService.Application.Services.Base
{
    public class BaseCRUDService<TAggregate> : IBaseCRUDService<TAggregate> where TAggregate : AggregateRoot<Guid>
    {
        public Task<T> GetOneByIDAsync<T>()
        {
            throw new NotImplementedException();
        }

        public Task<List<PagingData>> GetPagingAsync()
        {
            throw new NotImplementedException();
        }

        public Task<bool> SaveDataSync(TAggregate data)
        {
            throw new NotImplementedException();
        }
    }
}
