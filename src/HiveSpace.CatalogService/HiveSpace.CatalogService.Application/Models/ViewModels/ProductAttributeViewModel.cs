namespace HiveSpace.CatalogService.Application.Models.ViewModels
{
    public class ProductAttributeViewModel
    {
        public int AttributeId { get; set; }
        public string AttributeName { get; set; } = string.Empty;
        public int? GroupId { get; set; }
        public string? GroupName { get; set; }
        public List<int> SelectedValueIds { get; set; } = new();
        public string? FreeTextValue { get; set; }
        public List<string> NameValue { get; set; } = new();
    }
}
