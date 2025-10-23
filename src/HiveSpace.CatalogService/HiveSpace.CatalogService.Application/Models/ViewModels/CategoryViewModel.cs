namespace HiveSpace.CatalogService.Application.Models.ViewModels;

public class CategoryViewModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string FileImageId { get; set; } = string.Empty;
}
