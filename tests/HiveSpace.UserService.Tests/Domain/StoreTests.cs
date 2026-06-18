using FluentAssertions;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.UserService.Domain.Aggregates.Store;
using HiveSpace.UserService.Domain.Enums;
using Xunit;

namespace HiveSpace.UserService.Tests.Domain;

public class StoreTests
{
    private static Store NewStore(string name = "Valid Store", string? description = null)
        => Store.Create(name, description, "logo-file-id", "123 Main St, Hanoi", Guid.NewGuid(), null);

    [Fact]
    public void Create_WithValidFields_StartsInActiveStatus()
    {
        var store = NewStore();
        store.Status.Should().Be(StoreStatus.Active);
    }

    [Fact]
    public void Deactivate_ActiveStore_TransitionsToInactive()
    {
        var store = NewStore();
        store.Deactivate();
        store.Status.Should().Be(StoreStatus.Inactive);
    }

    [Fact]
    public void Activate_InactiveStore_TransitionsToActive()
    {
        var store = NewStore();
        store.Deactivate();
        store.Activate();
        store.Status.Should().Be(StoreStatus.Active);
    }

    [Fact]
    public void Create_WithWhitespaceName_ThrowsDomainException()
    {
        var act = () => Store.Create("   ", null, "logo", "address", Guid.NewGuid(), null);
        act.Should().Throw<InvalidFieldException>();
    }

    [Fact]
    public void Create_WithNameTooShort_ThrowsDomainException()
    {
        var act = () => Store.Create("A", null, "logo", "address", Guid.NewGuid(), null);
        act.Should().Throw<InvalidFieldException>();
    }

    [Fact]
    public void Create_WithDescriptionTooLong_ThrowsDomainException()
    {
        var longDesc = new string('a', 501);
        var act = () => Store.Create("Valid Store", longDesc, "logo", "address", Guid.NewGuid(), null);
        act.Should().Throw<InvalidFieldException>();
    }

    [Fact]
    public void Create_WithEmptyLogoFileId_ThrowsDomainException()
    {
        var act = () => Store.Create("Valid Store", null, "", "address", Guid.NewGuid(), null);
        act.Should().Throw<InvalidFieldException>();
    }

    [Fact]
    public void Create_WithEmptyAddress_ThrowsDomainException()
    {
        var act = () => Store.Create("Valid Store", null, "logo", "", Guid.NewGuid(), null);
        act.Should().Throw<InvalidFieldException>();
    }

    [Fact]
    public void Create_WithEmptyOwnerId_ThrowsDomainException()
    {
        var act = () => Store.Create("Valid Store", null, "logo", "address", Guid.Empty, null);
        act.Should().Throw<InvalidFieldException>();
    }

    [Fact]
    public void Create_WithNameExceedingMaxLength_ThrowsDomainException()
    {
        var longName = new string('a', 101);
        var act = () => Store.Create(longName, null, "logo", "address", Guid.NewGuid(), null);
        act.Should().Throw<InvalidFieldException>();
    }

    [Fact]
    public void Create_WithAddressExceedingMaxLength_ThrowsDomainException()
    {
        var longAddr = new string('a', 501);
        var act = () => Store.Create("Valid Store", null, "logo", longAddr, Guid.NewGuid(), null);
        act.Should().Throw<InvalidFieldException>();
    }

    [Fact]
    public void UpdateDetails_WithNewName_ChangesStoredName()
    {
        var store = NewStore("Original Name");
        store.UpdateDetails("New Name", null, null, null);
        store.StoreName.Should().Be("New Name");
    }

    [Fact]
    public void UpdateDetails_WithAllNullArguments_ChangesNothing()
    {
        var store = NewStore("Unchanged Name");
        store.UpdateDetails(null, null, null, null);
        store.StoreName.Should().Be("Unchanged Name");
    }

    [Fact]
    public void UpdateDetails_WithNewDescription_ChangesDescription()
    {
        var store = NewStore();
        store.UpdateDetails(null, "New description", null, null);
        store.Description.Should().Be("New description");
    }

    [Fact]
    public void UpdateDetails_WithNewLogoAndAddress_ChangesFields()
    {
        var store = NewStore();
        store.UpdateDetails(null, null, "new-logo-id", "New Address 123");
        store.LogoFileId.Should().Be("new-logo-id");
        store.Address.Should().Be("New Address 123");
    }

    [Fact]
    public void UpdateLogo_WithValidFileId_ChangesLogoFileId()
    {
        var store = NewStore();
        store.UpdateLogo("updated-logo-id");
        store.LogoFileId.Should().Be("updated-logo-id");
        store.LogoUrl.Should().BeNull();
    }

    [Fact]
    public void SetLogoUrl_SetsLogoUrl()
    {
        var store = NewStore();
        store.SetLogoUrl("https://cdn.example.com/logo.png");
        store.LogoUrl.Should().Be("https://cdn.example.com/logo.png");
    }

    [Fact]
    public void UpdateLogo_WithEmptyFileId_ThrowsDomainException()
    {
        var store = NewStore();
        var act = () => store.UpdateLogo("");
        act.Should().Throw<InvalidFieldException>();
    }

    [Fact]
    public void UpdateLogo_WithFileIdExceedingMaxLength_ThrowsDomainException()
    {
        var store = NewStore();
        var act = () => store.UpdateLogo(new string('a', 101));
        act.Should().Throw<InvalidFieldException>();
    }

    [Fact]
    public void Create_WithExplicitStoreId_UsesProvidedId()
    {
        var storeId = Guid.NewGuid();
        var store = Store.Create("Valid Store", null, "logo-file-id", "123 Main St", Guid.NewGuid(), storeId);
        store.Id.Should().Be(storeId);
    }
}
