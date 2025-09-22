using HiveSpace.CatalogService.Application.Interfaces;
using HiveSpace.CatalogService.Application.Interfaces.Base;
using HiveSpace.Domain.Shared.Entities;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace HiveSpace.CatalogService.API.Controllers.Base
{
    [ApiController]
    public class BaseCrudController<TAggregate, TService> : ControllerBase 
        where TService: IBaseCRUDService<TAggregate>
        where TAggregate : AggregateRoot<Guid>
    {
        private readonly TService _service;
        public BaseCrudController(TService service)
        {
            _service = service;
        }

        [HttpPost("paging")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetPaging()
        {
            var result = await _service.GetPagingAsync();
            return Ok(result);
        }
        [HttpGet("{id}")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetOneByID<T>(Guid id)
        {
            var result = await _service.GetOneByIDAsync<T>();
            return Ok(result);
        }

        [HttpPost]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<IActionResult> SaveDataSync(TAggregate data)
        {
            var result = await _service.SaveDataSync(data);
            return Ok(result);
        }
    }
}
