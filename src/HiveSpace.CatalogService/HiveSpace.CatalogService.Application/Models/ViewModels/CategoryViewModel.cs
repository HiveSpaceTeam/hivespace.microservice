namespace HiveSpace.CatalogService.Application.Models.ViewModels;

public record CategoryViewModel(
    int Id,
    string Name,
    string DisplayName,
    string FileImageId
);
