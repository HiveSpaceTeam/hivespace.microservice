namespace HiveSpace.CatalogService.Application.Models.ViewModels;

public class AttributeValueViewModel
{
    public Guid Id { get; set; }
    public Guid AttributeId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public Guid? ParentValueId { get; set; }
    public bool IsActive { get; set; }
    public int SortOrder { get; set; }
}


