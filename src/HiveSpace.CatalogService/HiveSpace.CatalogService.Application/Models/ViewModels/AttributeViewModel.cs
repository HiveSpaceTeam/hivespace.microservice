using HiveSpace.CatalogService.Domain.Aggregates.AttributeAggregate;

namespace HiveSpace.CatalogService.Application.Models.ViewModels;

public class AttributeViewModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public AttributeValueType ValueType { get; set; }
    public InputType InputType { get; set; }
    public bool IsMandatory { get; set; }
    public int MaxValueCount { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<AttributeValueViewModel> Values { get; set; } = new();
}


