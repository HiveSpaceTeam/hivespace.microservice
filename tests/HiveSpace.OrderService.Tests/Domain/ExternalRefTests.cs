using FluentAssertions;
using HiveSpace.Domain.Shared.Enumerations;
using HiveSpace.OrderService.Domain.Enumerations;
using HiveSpace.OrderService.Domain.External;
using Xunit;

namespace HiveSpace.OrderService.Tests.Domain;

public class ExternalRefTests
{
    // ── ProductRef ────────────────────────────────────────────────────────────

    [Fact]
    public void ProductRef_Constructor_StoresAllFields()
    {
        var storeId = Guid.NewGuid();

        var ref_ = new ProductRef(1L, storeId, "Widget", "thumb.jpg", ProductStatus.Available);

        ref_.Id.Should().Be(1L);
        ref_.StoreId.Should().Be(storeId);
        ref_.Name.Should().Be("Widget");
        ref_.ThumbnailUrl.Should().Be("thumb.jpg");
        ref_.Status.Should().Be(ProductStatus.Available);
        ref_.CreatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
        ref_.UpdatedAt.Should().BeNull();
    }

    [Fact]
    public void ProductRef_Update_ChangesAllMutableFields()
    {
        var ref_ = new ProductRef(1L, Guid.NewGuid(), "Old Name", null, ProductStatus.Available);
        var newStoreId = Guid.NewGuid();

        ref_.Update(newStoreId, "New Name", "new-thumb.jpg", ProductStatus.SoldOut);

        ref_.StoreId.Should().Be(newStoreId);
        ref_.Name.Should().Be("New Name");
        ref_.ThumbnailUrl.Should().Be("new-thumb.jpg");
        ref_.Status.Should().Be(ProductStatus.SoldOut);
    }

    // ── SkuRef ────────────────────────────────────────────────────────────────

    [Fact]
    public void SkuRef_Constructor_StoresAllFields()
    {
        var ref_ = new SkuRef(10L, 1L, "SKU-001", 50_000L, "VND", "img.jpg", "{}", "Red M");

        ref_.Id.Should().Be(10L);
        ref_.ProductId.Should().Be(1L);
        ref_.SkuNo.Should().Be("SKU-001");
        ref_.Price.Should().Be(50_000L);
        ref_.Currency.Should().Be("VND");
        ref_.ImageUrl.Should().Be("img.jpg");
        ref_.Attributes.Should().Be("{}");
        ref_.SkuName.Should().Be("Red M");
        ref_.CreatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
        ref_.UpdatedAt.Should().BeNull();
    }

    [Fact]
    public void SkuRef_Update_ChangesAllMutableFields()
    {
        var ref_ = new SkuRef(10L, 1L, "SKU-001", 50_000L, "VND", null, null);

        ref_.Update("SKU-002", 60_000L, "USD", "new.jpg", "{color:blue}", "Blue L");

        ref_.SkuNo.Should().Be("SKU-002");
        ref_.Price.Should().Be(60_000L);
        ref_.Currency.Should().Be("USD");
        ref_.ImageUrl.Should().Be("new.jpg");
        ref_.Attributes.Should().Be("{color:blue}");
        ref_.SkuName.Should().Be("Blue L");
    }

    // ── StoreRef ──────────────────────────────────────────────────────────────

    [Fact]
    public void StoreRef_Constructor_StoresAllFields()
    {
        var id = Guid.NewGuid();
        var ownerId = Guid.NewGuid();

        var ref_ = new StoreRef(id, "My Store", "logo.png", SellerStatus.Active, ownerId);

        ref_.Id.Should().Be(id);
        ref_.Name.Should().Be("My Store");
        ref_.LogoUrl.Should().Be("logo.png");
        ref_.Status.Should().Be(SellerStatus.Active);
        ref_.OwnerId.Should().Be(ownerId);
        ref_.CreatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
        ref_.UpdatedAt.Should().BeNull();
    }

    [Fact]
    public void StoreRef_Update_ChangesAllMutableFields()
    {
        var ref_ = new StoreRef(Guid.NewGuid(), "Old", null, SellerStatus.Active, Guid.NewGuid());

        ref_.Update("New Store", "new-logo.png", SellerStatus.Inactive);

        ref_.Name.Should().Be("New Store");
        ref_.LogoUrl.Should().Be("new-logo.png");
        ref_.Status.Should().Be(SellerStatus.Inactive);
    }
}
