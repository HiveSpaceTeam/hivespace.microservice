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
}
