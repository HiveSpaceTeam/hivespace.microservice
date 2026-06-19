using FluentAssertions;
using HiveSpace.Domain.Shared.Enumerations;
using HiveSpace.NotificationService.Core.DomainModels.External;
using Xunit;

namespace HiveSpace.NotificationService.Tests.Domain;

public class UserRefTests
{
    [Fact]
    public void Create_SetsAllProperties()
    {
        var id = Guid.NewGuid();
        var user = UserRef.Create(id, "user@test.com", "Test User",
            phoneNumber: "+84000", locale: Culture.Vi,
            userName: "testuser", avatarUrl: "https://img/a.png");

        user.Id.Should().Be(id);
        user.Email.Should().Be("user@test.com");
        user.FullName.Should().Be("Test User");
        user.PhoneNumber.Should().Be("+84000");
        user.Locale.Should().Be(Culture.Vi);
        user.UserName.Should().Be("testuser");
        user.AvatarUrl.Should().Be("https://img/a.png");
        user.StoreId.Should().BeNull();
        user.UpdatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void Update_ChangesContactDetails()
    {
        var user = UserRef.Create(Guid.NewGuid(), "old@test.com", "Old Name");
        user.Update("new@test.com", "New Name", "+84999", Culture.En);

        user.Email.Should().Be("new@test.com");
        user.FullName.Should().Be("New Name");
        user.PhoneNumber.Should().Be("+84999");
        user.Locale.Should().Be(Culture.En);
    }

    [Fact]
    public void UpdateStore_SetsStoreFields()
    {
        var user = UserRef.Create(Guid.NewGuid(), "seller@test.com", "Seller");
        var storeId = Guid.NewGuid();
        user.UpdateStore(storeId, "My Store", "https://img/logo.png");

        user.StoreId.Should().Be(storeId);
        user.StoreName.Should().Be("My Store");
        user.StoreLogoUrl.Should().Be("https://img/logo.png");
        user.UpdatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(2));
    }
}
