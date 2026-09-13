using FluentValidation;
using HiveSpace.CatalogService.Application.CatalogImports.Dtos;
using HiveSpace.CatalogService.Domain.Exceptions;
using HiveSpace.Core.Exceptions;
using HiveSpace.Core.Exceptions.Models;

namespace HiveSpace.CatalogService.Application.CatalogImports.Commands.ProvisionImportedCategories;

public class ProvisionImportedCategoriesValidator : AbstractValidator<ProvisionImportedCategoriesCommand>
{
    public ProvisionImportedCategoriesValidator()
    {
        RuleFor(x => x.Payload.SchemaVersion)
            .NotEmpty()
            .WithState(_ => new Error(CommonErrorCode.Required, nameof(CategoryProvisioningRequestDto.SchemaVersion)));

        RuleFor(x => x.Payload.Source.System)
            .NotEmpty()
            .WithState(_ => new Error(CommonErrorCode.Required, nameof(CategoryProvisioningSourceDto.System)));

        RuleFor(x => x.Payload.Source.Type)
            .Equal("sellercenter_categories")
            .WithState(_ => new Error(CatalogDomainErrorCode.InvalidImportedCategory, nameof(CategoryProvisioningSourceDto.Type)));

        RuleFor(x => x.Payload.Crawl.SourceFingerprint)
            .NotEmpty()
            .WithState(_ => new Error(CommonErrorCode.Required, nameof(CategoryProvisioningCrawlDto.SourceFingerprint)));

        RuleForEach(x => x.Payload.Categories).ChildRules(category =>
        {
            category.RuleFor(x => x.ExternalCategoryId)
                .NotEmpty()
                .WithState(_ => new Error(CommonErrorCode.Required, nameof(CategoryProvisioningCategoryDto.ExternalCategoryId)));
            category.RuleFor(x => x.Name)
                .NotEmpty()
                .WithState(_ => new Error(CommonErrorCode.Required, nameof(CategoryProvisioningCategoryDto.Name)));
        });
    }
}
