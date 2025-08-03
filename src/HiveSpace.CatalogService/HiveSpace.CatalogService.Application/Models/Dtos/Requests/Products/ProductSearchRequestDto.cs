namespace HiveSpace.CatalogService.Application.Models.Dtos.Request.Product
{
    public class ProductSearchRequestDto
    {
        public string Keyword { get; set; } = "";
        public string Sort { get; set; } = "ASC";
        public int PageSize { get; set; }
        public int PageIndex { get; set; }
    }
}
