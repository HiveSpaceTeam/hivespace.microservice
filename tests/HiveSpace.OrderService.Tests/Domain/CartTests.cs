using FluentAssertions;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.OrderService.Domain.Aggregates.Carts;
using Xunit;

namespace HiveSpace.OrderService.Tests.Domain;

public class CartTests
{
    public CartTests()
    {
        OrderIdGeneratorFixture.EnsureInitialized();
    }

    [Fact]
    public void AddItem_UpdatesCartCount()
    {
        var cart = Cart.Create(Guid.NewGuid(), Guid.NewGuid());

        cart.AddItem(1, 10, 2);

        cart.GetTotalItemCount().Should().Be(2);
    }

    [Fact]
    public void AddSameProductTwice_MergesIntoOneLine()
    {
        var cart = Cart.Create(Guid.NewGuid(), Guid.NewGuid());

        cart.AddItem(1, 10, 2);
        cart.AddItem(1, 10, 3);

        cart.Items.Should().ContainSingle();
        cart.GetTotalItemCount().Should().Be(5);
    }

    [Fact]
    public void UpdateQuantity_ChangesLineTotal()
    {
        var cart = Cart.Create(Guid.NewGuid(), Guid.NewGuid());
        cart.AddItem(1, 10, 2);

        cart.UpdateItemQuantity(1, 10, 4);

        cart.Items.Single().Quantity.Should().Be(4);
        cart.GetTotalItemCount().Should().Be(4);
    }

    [Fact]
    public void RemoveItem_LeavesRemainingItems()
    {
        var cart = Cart.Create(Guid.NewGuid(), Guid.NewGuid());
        cart.AddItem(1, 10, 2);
        cart.AddItem(2, 20, 1);

        cart.RemoveItem(1, 10);

        cart.Items.Should().ContainSingle(i => i.ProductId == 2);
    }

    [Fact]
    public void InitiateCheckoutOnEmptyCart_ThrowsDomainException()
    {
        var cart = Cart.Create(Guid.NewGuid(), Guid.NewGuid());

        var act = cart.ValidateForCheckout;

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_WithEmptyUserId_ThrowsDomainException()
    {
        var act = () => Cart.Create(Guid.Empty);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void AddItem_WithInvalidProductId_ThrowsDomainException()
    {
        var cart = Cart.Create(Guid.NewGuid());

        var act = () => cart.AddItem(0, 10, 1);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void AddItem_WithInvalidSkuId_ThrowsDomainException()
    {
        var cart = Cart.Create(Guid.NewGuid());

        var act = () => cart.AddItem(1, 0, 1);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void AddItem_WithInvalidQuantity_ThrowsDomainException()
    {
        var cart = Cart.Create(Guid.NewGuid());

        var act = () => cart.AddItem(1, 10, 0);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void UpdateItemQuantity_WithInvalidQuantity_ThrowsDomainException()
    {
        var cart = Cart.Create(Guid.NewGuid());
        cart.AddItem(1, 10, 2);

        var act = () => cart.UpdateItemQuantity(1, 10, 0);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void UpdateItemQuantity_WithNotFoundItem_ThrowsNotFoundException()
    {
        var cart = Cart.Create(Guid.NewGuid());

        var act = () => cart.UpdateItemQuantity(99, 99, 1);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void RemoveItem_WithNotFoundItem_ThrowsNotFoundException()
    {
        var cart = Cart.Create(Guid.NewGuid());

        var act = () => cart.RemoveItem(99, 99);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void RemoveItemById_WithValidId_RemovesItem()
    {
        var cart = Cart.Create(Guid.NewGuid());
        cart.AddItem(1, 10, 2);
        var itemId = cart.Items.Single().Id;

        cart.RemoveItemById(itemId);

        cart.IsEmpty().Should().BeTrue();
    }

    [Fact]
    public void RemoveItemById_WithNotFoundId_ThrowsNotFoundException()
    {
        var cart = Cart.Create(Guid.NewGuid());

        var act = () => cart.RemoveItemById(Guid.NewGuid());

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void UpdateItemById_WithAllOptions_UpdatesItem()
    {
        var cart = Cart.Create(Guid.NewGuid());
        cart.AddItem(1, 10, 2);
        var itemId = cart.Items.Single().Id;

        cart.UpdateItemById(itemId, skuId: 20L, quantity: 5, isSelected: false);

        var item = cart.Items.Single();
        item.SkuId.Should().Be(20L);
        item.Quantity.Should().Be(5);
        item.IsSelected.Should().BeFalse();
    }

    [Fact]
    public void UpdateItemById_WithNotFoundId_ThrowsNotFoundException()
    {
        var cart = Cart.Create(Guid.NewGuid());

        var act = () => cart.UpdateItemById(Guid.NewGuid(), null, null, null);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void UpdateItemById_WithNullOptions_DoesNotChange()
    {
        var cart = Cart.Create(Guid.NewGuid());
        cart.AddItem(1, 10, 2);
        var itemId = cart.Items.Single().Id;

        cart.UpdateItemById(itemId, null, null, null);

        var item = cart.Items.Single();
        item.SkuId.Should().Be(10L);
        item.Quantity.Should().Be(2);
    }

    [Fact]
    public void SelectAllItems_SelectsAll()
    {
        var cart = Cart.Create(Guid.NewGuid());
        cart.AddItem(1, 10, 1);
        cart.AddItem(2, 20, 1);
        cart.UpdateItemById(cart.Items.First().Id, null, null, isSelected: false);

        cart.SelectAllItems(true);

        cart.Items.Should().AllSatisfy(i => i.IsSelected.Should().BeTrue());
    }

    [Fact]
    public void SelectAllItems_DeselectsAll()
    {
        var cart = Cart.Create(Guid.NewGuid());
        cart.AddItem(1, 10, 1);
        cart.AddItem(2, 20, 1);

        cart.SelectAllItems(false);

        cart.Items.Should().AllSatisfy(i => i.IsSelected.Should().BeFalse());
    }

    [Fact]
    public void Clear_RemovesAllItemsAndCoupons()
    {
        var cart = Cart.Create(Guid.NewGuid());
        cart.AddItem(1, 10, 2);
        cart.ApplyPlatformCoupon("SAVE10");
        cart.ApplyStoreCoupon(Guid.NewGuid(), "STORE5");

        cart.Clear();

        cart.IsEmpty().Should().BeTrue();
        cart.AppliedPlatformCoupons.Should().BeEmpty();
        cart.AppliedStoreCoupons.Should().BeEmpty();
    }

    [Fact]
    public void ClearSelectedItems_RemovesOnlySelectedItems()
    {
        var cart = Cart.Create(Guid.NewGuid());
        cart.AddItem(1, 10, 1);
        cart.AddItem(2, 20, 1);
        cart.UpdateItemById(cart.Items.Last().Id, null, null, isSelected: false);

        cart.ClearSelectedItems();

        cart.Items.Should().ContainSingle(i => i.ProductId == 2);
    }

    [Fact]
    public void ClearSelectedItems_WithPurchasedStoreIds_ClearsPurchasedStoreCoupons()
    {
        var storeId = Guid.NewGuid();
        var cart = Cart.Create(Guid.NewGuid());
        cart.AddItem(1, 10, 1);
        cart.ApplyStoreCoupon(storeId, "STORE5");

        cart.ClearSelectedItems(new[] { storeId });

        cart.AppliedStoreCoupons.Should().BeEmpty();
    }

    [Fact]
    public void ClearSelectedItems_WithEmptyStoreIds_RetainsCoupons()
    {
        var storeId = Guid.NewGuid();
        var cart = Cart.Create(Guid.NewGuid());
        cart.AddItem(1, 10, 1);
        cart.ApplyStoreCoupon(storeId, "STORE5");

        cart.ClearSelectedItems(Array.Empty<Guid>());

        cart.AppliedStoreCoupons.Should().ContainSingle();
    }

    [Fact]
    public void ApplyPlatformCoupon_AddsCoupon()
    {
        var cart = Cart.Create(Guid.NewGuid());

        cart.ApplyPlatformCoupon("SAVE10");

        cart.AppliedPlatformCoupons.Should().ContainSingle(c => c.CouponCode == "SAVE10");
    }

    [Fact]
    public void ApplyPlatformCoupon_Duplicate_DoesNotAdd()
    {
        var cart = Cart.Create(Guid.NewGuid());
        cart.ApplyPlatformCoupon("SAVE10");

        cart.ApplyPlatformCoupon("save10");

        cart.AppliedPlatformCoupons.Should().ContainSingle();
    }

    [Fact]
    public void RemovePlatformCoupon_RemovesCoupon()
    {
        var cart = Cart.Create(Guid.NewGuid());
        cart.ApplyPlatformCoupon("SAVE10");

        cart.RemovePlatformCoupon("SAVE10");

        cart.AppliedPlatformCoupons.Should().BeEmpty();
    }

    [Fact]
    public void ApplyStoreCoupon_AddsStoreCoupon()
    {
        var storeId = Guid.NewGuid();
        var cart = Cart.Create(Guid.NewGuid());

        cart.ApplyStoreCoupon(storeId, "STORE5");

        cart.AppliedStoreCoupons.Should().ContainSingle(c => c.StoreId == storeId);
    }

    [Fact]
    public void ApplyStoreCoupon_UpdatesExistingStoreCoupon()
    {
        var storeId = Guid.NewGuid();
        var cart = Cart.Create(Guid.NewGuid());
        cart.ApplyStoreCoupon(storeId, "OLD");

        cart.ApplyStoreCoupon(storeId, "NEW");

        cart.AppliedStoreCoupons.Should().ContainSingle(c => c.CouponCode == "NEW");
    }

    [Fact]
    public void RemoveStoreCoupon_RemovesStoreCoupon()
    {
        var storeId = Guid.NewGuid();
        var cart = Cart.Create(Guid.NewGuid());
        cart.ApplyStoreCoupon(storeId, "STORE5");

        cart.RemoveStoreCoupon(storeId);

        cart.AppliedStoreCoupons.Should().BeEmpty();
    }

    [Fact]
    public void RemoveStoreCouponsWithoutSelectedItems_RemovesUnapplicableCoupons()
    {
        var storeId = Guid.NewGuid();
        var cart = Cart.Create(Guid.NewGuid());
        cart.AddItem(1, 10, 1);
        cart.UpdateItemById(cart.Items.Single().Id, null, null, isSelected: false);
        cart.ApplyStoreCoupon(storeId, "STORE5");

        cart.RemoveStoreCouponsWithoutSelectedItems(new Dictionary<long, Guid> { { 1L, storeId } });

        cart.AppliedStoreCoupons.Should().BeEmpty();
    }

    [Fact]
    public void RemoveStoreCouponsWithoutSelectedItems_WithNoStoreCoupons_DoesNothing()
    {
        var cart = Cart.Create(Guid.NewGuid());

        var act = () => cart.RemoveStoreCouponsWithoutSelectedItems(new Dictionary<long, Guid>());

        act.Should().NotThrow();
    }

    [Fact]
    public void IsEmpty_WithItems_ReturnsFalse()
    {
        var cart = Cart.Create(Guid.NewGuid());
        cart.AddItem(1, 10, 1);

        cart.IsEmpty().Should().BeFalse();
    }

    [Fact]
    public void IsEmpty_WithNoItems_ReturnsTrue()
    {
        var cart = Cart.Create(Guid.NewGuid());

        cart.IsEmpty().Should().BeTrue();
    }

    [Fact]
    public void Create_ExposesNullUpdatedAt()
    {
        var cart = Cart.Create(Guid.NewGuid());

        cart.UpdatedAt.Should().BeNull();
    }

    [Fact]
    public void ClearSelectedItems_WithAllGuidEmptyStoreIds_ReturnsEarly()
    {
        var storeId = Guid.NewGuid();
        var cart = Cart.Create(Guid.NewGuid());
        cart.AddItem(1, 10, 1);
        cart.ApplyStoreCoupon(storeId, "STORE5");

        // All Guid.Empty — filtered to empty set → early return without removing coupons
        cart.ClearSelectedItems(new[] { Guid.Empty }.ToList().AsReadOnly());

        cart.AppliedStoreCoupons.Should().ContainSingle();
    }

    [Fact]
    public void RemoveStoreCouponsWithoutSelectedItems_WithSelectedItems_RemovesMismatchedCoupons()
    {
        var storeId = Guid.NewGuid();
        var otherStoreId = Guid.NewGuid();
        var cart = Cart.Create(Guid.NewGuid());
        cart.AddItem(1, 10, 1);
        cart.ApplyStoreCoupon(otherStoreId, "OTHER");

        // Item 1 (productId=1) maps to storeId, but coupon is for otherStoreId
        cart.RemoveStoreCouponsWithoutSelectedItems(new Dictionary<long, Guid> { { 1L, storeId } });

        cart.AppliedStoreCoupons.Should().BeEmpty();
    }
}
