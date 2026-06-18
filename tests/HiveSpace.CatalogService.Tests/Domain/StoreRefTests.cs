using FluentAssertions;
using HiveSpace.CatalogService.Domain.Aggregates.External;
using Xunit;

namespace HiveSpace.CatalogService.Tests.Domain;

public class StoreRefTests
{
    [Fact]
    public void Create_SetsAllProperties()
    {
        var id = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;
        var store = new StoreRef(id, ownerId, "My Store", "A great store", "http://logo.url", "123 Main St", now, now);
        store.Id.Should().Be(id);
        store.OwnerId.Should().Be(ownerId);
        store.StoreName.Should().Be("My Store");
        store.Description.Should().Be("A great store");
        store.LogoUrl.Should().Be("http://logo.url");
        store.Address.Should().Be("123 Main St");
    }

    [Fact]
    public void Update_ChangesAllMutableFields()
    {
        var store = new StoreRef(Guid.NewGuid(), Guid.NewGuid(), "Old Name", null, null, "Old Address",
            DateTimeOffset.UtcNow, DateTimeOffset.UtcNow);
        store.Update("New Name", "A description", "http://new-logo.url", "New Address");
        store.StoreName.Should().Be("New Name");
        store.Description.Should().Be("A description");
        store.LogoUrl.Should().Be("http://new-logo.url");
        store.Address.Should().Be("New Address");
        store.UpdatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
    }
}
