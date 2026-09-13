using FluentAssertions;
using HiveSpace.CatalogService.Application.CatalogImports.Commands.ImportReadyProducts;
using HiveSpace.Core.Exceptions.Models;
using HiveSpace.Domain.Shared.Enumerations;
using Xunit;

namespace HiveSpace.CatalogService.Tests.Application.CatalogImports;

public class ImportReadyProductsValidatorTests
{
    [Theory]
    [InlineData(nameof(ProductStatus.Available))]
    [InlineData(nameof(ProductStatus.Draft))]
    [InlineData(nameof(ProductStatus.Unpublish))]
    public void Validate_WithSupportedState_IsValid(string publicationState)
    {
        var validator = new ImportReadyProductsValidator();
        var command = new ImportReadyProductsCommand(Guid.NewGuid(), null, publicationState, Guid.NewGuid());

        var result = validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("Archived")]
    [InlineData("Inactive")]
    public void Validate_WithUnsupportedState_IsInvalid(string? publicationState)
    {
        var validator = new ImportReadyProductsValidator();
        var command = new ImportReadyProductsCommand(Guid.NewGuid(), null, publicationState!, Guid.NewGuid());

        var result = validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle();
        var state = result.Errors.Single().CustomState.Should().BeOfType<Error>().Subject;
        state.Source.Should().Be(nameof(ImportReadyProductsCommand.PublicationState));
    }
}
