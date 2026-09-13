using FluentValidation;
using HiveSpace.CatalogService.Application.CatalogImports.Dtos;
using HiveSpace.Core.Exceptions;
using HiveSpace.Core.Exceptions.Models;

namespace HiveSpace.CatalogService.Application.CatalogImports.Commands.ProvisionImportedCategoryAttributes;

public class ProvisionImportedCategoryAttributesValidator : AbstractValidator<ProvisionImportedCategoryAttributesCommand>
{
    public ProvisionImportedCategoryAttributesValidator()
    {
        RuleFor(x => x.Payload.SchemaVersion)
            .NotEmpty()
            .WithState(_ => new Error(CommonErrorCode.Required, nameof(CategoryAttributeProvisioningRequestDto.SchemaVersion)));

        RuleFor(x => x.Payload.Source.System)
            .NotEmpty()
            .WithState(_ => new Error(CommonErrorCode.Required, nameof(CategoryProvisioningSourceDto.System)));

        RuleFor(x => x.Payload.Categories)
            .NotEmpty()
            .WithState(_ => new Error(CommonErrorCode.Required, nameof(CategoryAttributeProvisioningRequestDto.Categories)));

        RuleForEach(x => x.Payload.Categories).ChildRules(category =>
        {
            category.RuleFor(x => x.ExternalCategoryId)
                .NotEmpty()
                .WithState(_ => new Error(CommonErrorCode.Required, nameof(CategoryAttributeProvisioningCategoryDto.ExternalCategoryId)));

            category.RuleFor(x => x.Attributes)
                .NotEmpty()
                .WithState(_ => new Error(CommonErrorCode.Required, nameof(CategoryAttributeProvisioningCategoryDto.Attributes)));

            category.RuleForEach(x => x.Attributes).ChildRules(attribute =>
            {
                attribute.RuleFor(x => x.SourceAttributeId)
                    .NotEmpty()
                    .WithState(_ => new Error(CommonErrorCode.Required, nameof(CategoryAttributeProvisioningAttributeDto.SourceAttributeId)));
                attribute.RuleFor(x => x.Name)
                    .NotEmpty()
                    .WithState(_ => new Error(CommonErrorCode.Required, nameof(CategoryAttributeProvisioningAttributeDto.Name)));
                attribute.RuleFor(x => x.InputType)
                    .NotEmpty()
                    .WithState(_ => new Error(CommonErrorCode.Required, nameof(CategoryAttributeProvisioningAttributeDto.InputType)));

                attribute.RuleForEach(x => x.Values).ChildRules(value =>
                {
                    value.RuleFor(x => x.SourceValueId)
                        .NotEmpty()
                        .WithState(_ => new Error(CommonErrorCode.Required, nameof(CategoryAttributeProvisioningAttributeValueDto.SourceValueId)));
                    value.RuleFor(x => x.Name)
                        .NotEmpty()
                        .WithState(_ => new Error(CommonErrorCode.Required, nameof(CategoryAttributeProvisioningAttributeValueDto.Name)));
                    value.RuleFor(x => x.DisplayName)
                        .NotEmpty()
                        .WithState(_ => new Error(CommonErrorCode.Required, nameof(CategoryAttributeProvisioningAttributeValueDto.DisplayName)));
                });
            });
        });
    }
}
