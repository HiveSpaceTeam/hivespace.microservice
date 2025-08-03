using Xunit;
using FluentAssertions;
using HiveSpace.IdentityService.Domain.Aggregates;
using HiveSpace.IdentityService.Domain.Aggregates.Enums;
using HiveSpace.IdentityService.Domain.DomainEvents;
using HiveSpace.IdentityService.Domain.Exceptions;
using HiveSpace.Domain.Shared.Interfaces;
using System;
using System.Linq;

namespace HiveSpace.IdentityService.UnitTests.Domain;

public class ApplicationUserTests
{
    [Fact]
    public void Constructor_ValidInput_InitializesPropertiesAndAddsUserCreatedDomainEvent()
    {
        // Arrange
        string email = "test@example.com";
        string userName = "testuser";
        string fullName = "Test User";
        string phoneNumber = "123-456-7890";
        Gender gender = Gender.Male;
        DateTimeOffset dob = new DateTimeOffset(1990, 1, 1, 0, 0, 0, TimeSpan.Zero);

        // Act
        var user = new ApplicationUser(email, userName, fullName, phoneNumber, gender, dob);

        // Assert
        user.Should().NotBeNull();
        user.Email.Should().Be(email);
        user.UserName.Should().Be(userName);
        user.FullName.Should().Be(fullName);
        user.PhoneNumber.Should().Be(phoneNumber);
        user.Gender.Should().Be(gender);
        user.DateOfBirth.Should().Be(dob);
        user.IsDeleted.Should().BeFalse();
        user.UpdatedAt.Should().BeNull();
        user.DeletedAt.Should().BeNull();

        user.DomainEvents.Should().HaveCount(1);
        var userCreatedEvent = user.DomainEvents.First().Should().BeOfType<UserCreatedDomainEvent>().Subject;
        userCreatedEvent.UserId.Should().Be(Guid.Parse(user.Id));
        userCreatedEvent.Email.Should().Be(email);
        userCreatedEvent.FullName.Should().Be(fullName);
    }

    [Theory]
    [InlineData(null, null, null, null, null, null)]
    [InlineData("new@example.com", "newusername", "New FullName", "987-654-3210", Gender.Female, "1985-05-10T00:00:00Z")]
    public void UpdateUserInfo_ValidInput_UpdatesPropertiesCorrectly(string? newEmail, string? newUserName, string? newFullName, string? newPhoneNumber, Gender? newGender, string? newDobString)
    {
        // Arrange
        var user = new ApplicationUser("original@example.com", "originaluser", "Original User", "111-222-3333", Gender.Male, new DateTimeOffset(2000, 1, 1, 0, 0, 0, TimeSpan.Zero));
        DateTimeOffset? newDob = newDobString != null ? DateTimeOffset.Parse(newDobString) : null;

        // Act
        user.UpdateUserInfo(newUserName, newFullName, newEmail, newPhoneNumber, newGender, newDob);

        // Assert
        if (newEmail != null) user.Email.Should().Be(newEmail);
        if (newUserName != null) user.UserName.Should().Be(newUserName);
        if (newFullName != null) user.FullName.Should().Be(newFullName);
        if (newPhoneNumber != null) user.PhoneNumber.Should().Be(newPhoneNumber);
        if (newGender != null) user.Gender.Should().Be(newGender);
        if (newDob != null) user.DateOfBirth.Should().Be(newDob);
    }

    [Fact]
    public void AddAddress_ValidAddressProps_AddsAddressToList()
    {
        // Arrange
        var user = new ApplicationUser("test@example.com", "testuser", "Test User", null);
        var addressProps = new AddressProps
        {
            FullName = "Test FullName",
            Street = "Test Street",
            Ward = "Test Ward",
            District = "Test District",
            Province = "Test Province",
            Country = "Test Country",
            ZipCode = "12345",
            PhoneNumber = "0987654321"
        };

        // Act
        var address = user.AddAddress(addressProps);

        // Assert
        user.Addresses.Should().Contain(address);
        user.Addresses.Should().HaveCount(1);
        address.Should().NotBeNull();
        address.Street.Should().Be(addressProps.Street);
        address.District.Should().Be(addressProps.District);
    }

    [Fact]
    public void RemoveAddress_ExistingAddressId_RemovesAddressFromList()
    {
        // Arrange
        var user = new ApplicationUser("test@example.com", "testuser", "Test User", null);
        var addressProps1 = new AddressProps { FullName = "Addr1", Street = "Street1", Ward = "Ward1", District = "Dist1", Province = "Prov1", Country = "Country1", ZipCode = "1", PhoneNumber = "1" };
        var addressProps2 = new AddressProps { FullName = "Addr2", Street = "Street2", Ward = "Ward2", District = "Dist2", Province = "Prov2", Country = "Country2", ZipCode = "2", PhoneNumber = "2" };
        var address1 = user.AddAddress(addressProps1);
        var address2 = user.AddAddress(addressProps2);

        // Act
        user.RemoveAddress(address1.Id);

        // Assert
        user.Addresses.Should().HaveCount(1);
        user.Addresses.First().Should().Be(address2);
    }

    [Fact]
    public void RemoveAddress_NonExistingAddressId_ThrowsAddressNotFoundException()
    {
        // Arrange
        var user = new ApplicationUser("test@example.com", "testuser", "Test User", null);

        // Act
        Action act = () => user.RemoveAddress(Guid.NewGuid());

        // Assert
        act.Should().Throw<AddressNotFoundException>();
    }

    [Fact]
    public void UpdateAddress_ExistingAddressIdAndValidProps_UpdatesAddressCorrectly()
    {
        // Arrange
        var user = new ApplicationUser("test@example.com", "testuser", "Test User", null);
        var oldAddressProps = new AddressProps { FullName = "Old FullName", Street = "Old Street", Ward = "Old Ward", District = "Old District", Province = "Old Province", Country = "Old Country", ZipCode = "Old Zip", PhoneNumber = "Old Phone" };
        var address = user.AddAddress(oldAddressProps);
        var newAddressProps = new AddressProps { FullName = "New FullName", Street = "New Street", Ward = "New Ward", District = "New District", Province = "New Province", Country = "New Country", ZipCode = "New Zip", PhoneNumber = "New Phone" };

        // Act
        var updatedAddress = user.UpdateAddress(address.Id, newAddressProps);

        // Assert
        updatedAddress.Should().Be(address);
        updatedAddress.Street.Should().Be(newAddressProps.Street);
        updatedAddress.District.Should().Be(newAddressProps.District);
    }

    [Fact]
    public void UpdateAddress_NonExistingAddressId_ThrowsAddressNotFoundException()
    {
        // Arrange
        var user = new ApplicationUser("test@example.com", "testuser", "Test User", null);
        var newAddressProps = new AddressProps { FullName = "FN", Street = "St", Ward = "Wd", District = "Dt", Province = "Pv", Country = "Cy", ZipCode = "Zc", PhoneNumber = "Pn" };

        // Act
        Action act = () => user.UpdateAddress(Guid.NewGuid(), newAddressProps);

        // Assert
        act.Should().Throw<AddressNotFoundException>();
    }

    [Fact]
    public void SetDefaultAddress_ExistingAddressId_SetsCorrectAddressAsDefault()
    {
        // Arrange
        var user = new ApplicationUser("test@example.com", "testuser", "Test User", null);
        var addressProps1 = new AddressProps { FullName = "Addr1", Street = "Street1", Ward = "Ward1", District = "Dist1", Province = "Prov1", Country = "Country1", ZipCode = "1", PhoneNumber = "1" };
        var addressProps2 = new AddressProps { FullName = "Addr2", Street = "Street2", Ward = "Ward2", District = "Dist2", Province = "Prov2", Country = "Country2", ZipCode = "2", PhoneNumber = "2" };
        var address1 = user.AddAddress(addressProps1);
        var address2 = user.AddAddress(addressProps2);

        // Act
        user.SetDefaultAddress(address2.Id);

        // Assert
        address1.IsDefault.Should().BeFalse();
        address2.IsDefault.Should().BeTrue();
    }

    [Fact]
    public void SetDefaultAddress_NonExistingAddressId_ThrowsAddressNotFoundException()
    {
        // Arrange
        var user = new ApplicationUser("test@example.com", "testuser", "Test User", null);

        // Act
        Action act = () => user.SetDefaultAddress(Guid.NewGuid());

        // Assert
        act.Should().Throw<AddressNotFoundException>();
    }

    [Fact]
    public void ClearDomainEvents_WithEvents_ClearsAllDomainEvents()
    {
        // Arrange
        var user = new ApplicationUser("test@example.com", "testuser", "Test User", null);
        var event1 = new UserCreatedDomainEvent(Guid.NewGuid(), "test@example.com", "Test User");
        var event2 = new UserCreatedDomainEvent(Guid.NewGuid(), "test2@example.com", "Test User2");
        user.AddDomainEvent(event1);
        user.AddDomainEvent(event2);

        // Act
        user.ClearDomainEvents();

        // Assert
        user.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void RemoveDomainEvent_ExistingEvent_RemovesSpecificDomainEvent()
    {
        // Arrange
        var user = new ApplicationUser("test@example.com", "testuser", "Test User", null);
        var event1 = new UserCreatedDomainEvent(Guid.NewGuid(), "test@example.com", "Test User");
        var event2 = new UserCreatedDomainEvent(Guid.NewGuid(), "test2@example.com", "Test User2");
        user.AddDomainEvent(event1);
        user.AddDomainEvent(event2);

        // Act
        user.RemoveDomainEvent(event1);

        // Assert
        user.DomainEvents.Should().NotContain(event1);
        user.DomainEvents.Should().ContainSingle(e => e.Equals(event2));
    }
}
