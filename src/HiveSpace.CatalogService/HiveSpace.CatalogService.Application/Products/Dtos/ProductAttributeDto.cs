namespace HiveSpace.CatalogService.Application.Products.Dtos;

public record ProductAttributeDto
{
    public int AttributeId { get; init; }
    public string AttributeName { get; init; } = string.Empty;
    public int? GroupId { get; init; }
    public string? GroupName { get; init; }
    public List<int> SelectedValueIds { get; init; } = [];
    public string? FreeTextValue { get; init; }
    public List<string> NameValue { get; init; } = [];
}
