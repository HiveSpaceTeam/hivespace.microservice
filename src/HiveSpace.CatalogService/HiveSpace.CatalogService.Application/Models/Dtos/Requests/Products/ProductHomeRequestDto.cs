using System.ComponentModel.DataAnnotations;

namespace HiveSpace.CatalogService.Application.Models.Dtos.Request.Product
{
    public class ProductHomeRequestDto
    {
        [Range(1, 100)]
        public int PageSize { get; set; }

        [Range(0, int.MaxValue)]
        public int PageIndex { get; set; }
    }
}
