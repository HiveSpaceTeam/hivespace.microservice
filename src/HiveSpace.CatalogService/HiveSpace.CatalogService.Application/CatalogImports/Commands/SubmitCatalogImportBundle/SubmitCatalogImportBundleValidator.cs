using FluentValidation;
using HiveSpace.CatalogService.Application.CatalogImports.Dtos;
using HiveSpace.CatalogService.Domain.Exceptions;
using HiveSpace.Core.Exceptions;
using HiveSpace.Core.Exceptions.Models;

namespace HiveSpace.CatalogService.Application.CatalogImports.Commands.SubmitCatalogImportBundle;

public class SubmitCatalogImportBundleValidator : AbstractValidator<SubmitCatalogImportBundleCommand>
{
    public SubmitCatalogImportBundleValidator()
    {
        RuleFor(x => x.Payload.SchemaVersion)
            .NotEmpty()
            .WithState(_ => new Error(CommonErrorCode.Required, nameof(CatalogImportBundleRequestDto.SchemaVersion)));

        RuleFor(x => x.Payload.Source.System)
            .NotEmpty()
            .WithState(_ => new Error(CommonErrorCode.Required, nameof(CatalogImportSourceDto.System)));

        RuleFor(x => x.Payload.Source.Type)
            .NotEmpty()
            .WithState(_ => new Error(CommonErrorCode.Required, nameof(CatalogImportSourceDto.Type)))
            .Must(type => type is "category" or "search" or "product_list")
            .WithState(_ => new Error(CatalogDomainErrorCode.InvalidCatalogImportBundle, nameof(CatalogImportSourceDto.Type)));

        RuleFor(x => x.Payload.Source.Value)
            .NotEmpty()
            .WithState(_ => new Error(CommonErrorCode.Required, nameof(CatalogImportSourceDto.Value)));

        RuleFor(x => x.Payload.Crawl.SourceFingerprint)
            .NotEmpty()
            .WithState(_ => new Error(CommonErrorCode.Required, nameof(CatalogImportCrawlDto.SourceFingerprint)));

        RuleForEach(x => x.Payload.Sellers).ChildRules(seller =>
        {
            seller.RuleFor(x => x.ExternalSellerId)
                .NotEmpty()
                .WithState(_ => new Error(CommonErrorCode.Required, nameof(ImportedSellerRequestDto.ExternalSellerId)));

            seller.RuleFor(x => x.DisplayName)
                .NotEmpty()
                .WithState(_ => new Error(CommonErrorCode.Required, nameof(ImportedSellerRequestDto.DisplayName)));

            seller.RuleFor(x => x.LogoUrl)
                .NotEmpty()
                .WithState(_ => new Error(CommonErrorCode.Required, nameof(ImportedSellerRequestDto.LogoUrl)));
        });
    }
}
