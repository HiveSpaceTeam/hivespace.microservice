using HiveSpace.CatalogService.Application.Models.Dtos.Crud;
using HiveSpace.Domain.Shared.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HiveSpace.CatalogService.Application.Interfaces.Base
{
    public interface IBaseCRUDService<TAggregate> where TAggregate : AggregateRoot<Guid>
    {
        Task<List<PagingData>> GetPagingAsync();

        Task<T> GetOneByIDAsync<T>();

        Task<bool> SaveDataSync(TAggregate data);
    }
}
