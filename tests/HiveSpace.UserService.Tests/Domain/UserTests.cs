using FluentAssertions;
using HiveSpace.Domain.Shared.Enumerations;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.UserService.Domain.Aggregates.User;
using HiveSpace.UserService.Domain.Enums;
using HiveSpace.UserService.Domain.Exceptions;
using Xunit;

namespace HiveSpace.UserService.Tests.Domain;

public class UserTests
{
    private static Email ValidEmail(string addr = "test@example.com") => Email.Create(addr);

    [Fact]
    public void Create_WithUsernameTooShort_ThrowsInvalidUserInformationException()
    {
        var act = () => User.CreateProfile(Guid.NewGuid(), ValidEmail(), "ab", "Valid Name");
        act.Should().Throw<InvalidUserInformationException>();
    }

    [Fact]
    public void Create_WithUsernameTooLong_ThrowsInvalidUserInformationException()
    {
        var longName = new string('a', 51);
        var act = () => User.CreateProfile(Guid.NewGuid(), ValidEmail(), longName, "Valid Name");
        act.Should().Throw<InvalidUserInformationException>();
    }

    [Fact]
    public void Create_WithUsernameContainingInvalidCharacters_ThrowsInvalidUserInformationException()
    {
        var act = () => User.CreateProfile(Guid.NewGuid(), ValidEmail(), "user#invalid!", "Valid Name");
        act.Should().Throw<InvalidUserInformationException>();
    }

    [Fact]
    public void UpdateProfile_WithValidFields_ChangesStoredValues()
    {
        var user = User.CreateProfile(Guid.NewGuid(), ValidEmail(), "validuser", "Old Name");
        user.UpdateProfile("New Full Name", null, null, null, "newusername");
        user.FullName.Should().Be("New Full Name");
        user.UserName.Should().Be("newusername");
    }

    [Fact]
    public void MarkAddressAsDefault_ClearsPreviousDefaultFlag()
    {
        var user = User.CreateProfile(Guid.NewGuid(), ValidEmail(), "validuser2", "Valid User");
        var first = user.AddAddress("Name", "0900000000", "Street 1", "Ward", "Hanoi", "VN", null, AddressType.Home, true);
        var second = user.AddAddress("Name", "0900000000", "Street 2", "Ward", "Hanoi", "VN", null, AddressType.Work);

        user.MarkAddressAsDefault(second.Id);

        first.IsDefault.Should().BeFalse();
        second.IsDefault.Should().BeTrue();
    }

    [Fact]
    public void RemoveAddress_WhenOnlyAddress_ThrowsCannotRemoveOnlyAddressException()
    {
        var user = User.CreateProfile(Guid.NewGuid(), ValidEmail("only@example.com"), "onlyuser", "Only User");
        var address = user.AddAddress("Name", "0900000000", "Street 1", "Ward", "Hanoi", "VN", null, AddressType.Home);

        var act = () => user.RemoveAddress(address.Id);
        act.Should().Throw<CannotRemoveOnlyAddressException>();
    }

    [Fact]
    public void RemoveAddress_WhenAddressIsDefault_ThrowsCannotRemoveDefaultAddressException()
    {
        var user = User.CreateProfile(Guid.NewGuid(), ValidEmail("rmdefault@example.com"), "rmdefaultuser", "Rm Default");
        var defaultAddr = user.AddAddress("Name", "0900000000", "Street 1", "Ward", "Hanoi", "VN", null, AddressType.Home, true);
        user.AddAddress("Name", "0900000000", "Street 2", "Ward", "Hanoi", "VN", null, AddressType.Work);

        var act = () => user.RemoveAddress(defaultAddr.Id);
        act.Should().Throw<CannotRemoveDefaultAddressException>();
    }

    [Fact]
    public void SetAvatar_WithEmptyFileId_ThrowsInvalidFieldException()
    {
        var user = User.CreateProfile(Guid.NewGuid(), ValidEmail(), "avataruser", "Avatar User");

        var act = () => user.SetAvatar("");
        act.Should().Throw<InvalidFieldException>();
    }

    [Fact]
    public void UpdateProfile_WithInvalidUsernameTooShort_ThrowsInvalidFieldException()
    {
        var user = User.CreateProfile(Guid.NewGuid(), ValidEmail(), "validuser5", "Valid User");

        var act = () => user.UpdateProfile(null, null, null, null, "ab");
        act.Should().Throw<InvalidFieldException>();
    }

    [Fact]
    public void Create_WithNullEmail_ThrowsInvalidUserInformationException()
    {
        var act = () => User.CreateProfile(Guid.NewGuid(), null!, "validuser6", "Valid User");
        act.Should().Throw<InvalidUserInformationException>();
    }

    [Fact]
    public void Create_WithFullNameTooShort_ThrowsInvalidUserInformationException()
    {
        var act = () => User.CreateProfile(Guid.NewGuid(), ValidEmail(), "validuser7", "X");
        act.Should().Throw<InvalidUserInformationException>();
    }

    [Fact]
    public void SetAvatarUrl_WithUrl_SetsAvatarUrl()
    {
        var user = User.CreateProfile(Guid.NewGuid(), ValidEmail(), "avatarurluser", "Avatar User");

        user.SetAvatarUrl("https://cdn.example.com/avatar.jpg");

        user.AvatarUrl.Should().Be("https://cdn.example.com/avatar.jpg");
        user.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void UpdateAddress_WithUnknownId_ThrowsNotFoundException()
    {
        var user = User.CreateProfile(Guid.NewGuid(), ValidEmail(), "addruser1", "Addr User");

        var act = () => user.UpdateAddress(Guid.NewGuid(), "New Name", null, null, null, null, null, null, null);
        act.Should().Throw<NotFoundException>();
    }

    [Fact]
    public void UpdateAddress_WithValidId_SetsUpdatedAt()
    {
        var user = User.CreateProfile(Guid.NewGuid(), ValidEmail(), "addruser2", "Addr User");
        var address = user.AddAddress("Name", "0900000000", "Street 1", "Ward", "Hanoi", "VN", null, AddressType.Home);

        user.UpdateAddress(address.Id, "New Name", null, null, null, null, null, null, null);

        user.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void UpdateTheme_ChangesThemeInSettings()
    {
        var user = User.CreateProfile(Guid.NewGuid(), ValidEmail(), "themeuser", "Theme User");

        user.UpdateTheme(Theme.Dark);

        user.Settings.Theme.Should().Be(Theme.Dark);
        user.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void UpdateCulture_ChangesCultureInSettings()
    {
        var user = User.CreateProfile(Guid.NewGuid(), ValidEmail(), "cultureuser", "Culture User");

        user.UpdateCulture(Culture.Vi);

        user.Settings.Culture.Should().Be(Culture.Vi);
        user.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void UpdateProfile_WithPhoneAndGender_UpdatesThoseFields()
    {
        var user = User.CreateProfile(Guid.NewGuid(), ValidEmail(), "profuser1", "Profile User");
        var phone = PhoneNumber.Create("5551234567");

        user.UpdateProfile(null, phone, null, Gender.Male);

        user.PhoneNumber.Should().NotBeNull();
        user.Gender.Should().Be(Gender.Male);
        user.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void Create_WithWhitespaceUsername_ThrowsInvalidUserInformationException()
    {
        var act = () => User.CreateProfile(Guid.NewGuid(), ValidEmail(), "   ", "Valid Name");
        act.Should().Throw<InvalidUserInformationException>();
    }

    [Fact]
    public void Create_WithWhitespaceFullName_ThrowsInvalidUserInformationException()
    {
        var act = () => User.CreateProfile(Guid.NewGuid(), ValidEmail(), "validuser8", "   ");
        act.Should().Throw<InvalidUserInformationException>();
    }

    [Fact]
    public void Create_WithFullNameTooLong_ThrowsInvalidUserInformationException()
    {
        var longName = new string('a', 101);
        var act = () => User.CreateProfile(Guid.NewGuid(), ValidEmail(), "validuser9", longName);
        act.Should().Throw<InvalidUserInformationException>();
    }

    [Fact]
    public void RemoveAddress_WhenNonDefaultAndNotOnly_RemovesSuccessfully()
    {
        var user = User.CreateProfile(Guid.NewGuid(), ValidEmail(), "removeuser", "Remove User");
        var first = user.AddAddress("Name", "0900000000", "Street 1", "Ward", "Hanoi", "VN", null, AddressType.Home, true);
        var second = user.AddAddress("Name", "0900000000", "Street 2", "Ward", "Hanoi", "VN", null, AddressType.Work);

        user.RemoveAddress(second.Id);

        user.Addresses.Should().HaveCount(1);
        user.Addresses.Should().NotContain(a => a.Id == second.Id);
    }

    [Fact]
    public void MarkAddressAsDefault_WithUnknownId_ThrowsNotFoundException()
    {
        var user = User.CreateProfile(Guid.NewGuid(), ValidEmail(), "markdefaultuser", "Mark Default User");

        var act = () => user.MarkAddressAsDefault(Guid.NewGuid());

        act.Should().Throw<NotFoundException>();
    }

    [Fact]
    public void UpdateProfile_WithDateOfBirth_SetsDateOfBirth()
    {
        var user = User.CreateProfile(Guid.NewGuid(), ValidEmail(), "dobuser", "Dob User");
        var dob = new DateOfBirth(new DateTimeOffset(1990, 1, 1, 0, 0, 0, TimeSpan.Zero));

        user.UpdateProfile(null, null, dob, null);

        user.DateOfBirth.Should().NotBeNull();
        user.DateOfBirth!.Value.Should().Be(dob.Value);
    }

    [Fact]
    public void AddAddress_WithSetAsDefault_ClearsExistingDefault()
    {
        var user = User.CreateProfile(Guid.NewGuid(), ValidEmail(), "defaultaddr", "Default Addr User");
        var first = user.AddAddress("Name", "0900000000", "Street 1", "Ward", "City", "VN", null, AddressType.Home, true);

        var second = user.AddAddress("Name", "0900000000", "Street 2", "Ward", "City", "VN", null, AddressType.Work, true);

        first.IsDefault.Should().BeFalse();
        second.IsDefault.Should().BeTrue();
    }

    [Fact]
    public void UpdateProfile_WithUsernameTooLong_ThrowsInvalidFieldException()
    {
        var user = User.CreateProfile(Guid.NewGuid(), ValidEmail(), "longuser1", "Long User");
        var longName = new string('a', 51);

        var act = () => user.UpdateProfile(null, null, null, null, longName);

        act.Should().Throw<InvalidFieldException>();
    }

    [Fact]
    public void UpdateProfile_WithInvalidCharsInUsername_ThrowsInvalidFieldException()
    {
        var user = User.CreateProfile(Guid.NewGuid(), ValidEmail(), "invalidcharuser", "Invalid Char User");

        var act = () => user.UpdateProfile(null, null, null, null, "user#bad!");

        act.Should().Throw<InvalidFieldException>();
    }

    [Fact]
    public void UpdateProfile_WithUnderscoreInUsername_UpdatesUserName()
    {
        var user = User.CreateProfile(Guid.NewGuid(), ValidEmail(), "validuser10", "Valid User");
        user.UpdateProfile(null, null, null, null, "valid_user");
        user.UserName.Should().Be("valid_user");
    }

    [Fact]
    public void Create_WithAvatarUrl_SetsAvatarUrl()
    {
        var user = User.CreateProfile(Guid.NewGuid(), ValidEmail(), "avatarurlcreate", "Avatar User", avatarUrl: "  https://cdn.example.com/avatar.jpg  ");

        user.AvatarUrl.Should().Be("https://cdn.example.com/avatar.jpg");
    }

    [Fact]
    public void Create_WithCreatedAt_UsesProvidedCreatedAt()
    {
        var createdAt = new DateTimeOffset(2020, 6, 15, 12, 0, 0, TimeSpan.Zero);
        var user = User.CreateProfile(Guid.NewGuid(), ValidEmail(), "createdat1", "Created At User", createdAt: createdAt);

        user.CreatedAt.Should().Be(createdAt);
    }

    [Fact]
    public void RemoveAddress_WithNonExistentId_ThrowsNotFoundException()
    {
        var user = User.CreateProfile(Guid.NewGuid(), ValidEmail(), "removenotfound", "Remove User");
        user.AddAddress("Name", "0900000000", "Street 1", "Ward", "Hanoi", "VN", null, AddressType.Home, true);
        user.AddAddress("Name", "0900000000", "Street 2", "Ward", "Hanoi", "VN", null, AddressType.Work);

        var act = () => user.RemoveAddress(Guid.NewGuid());

        act.Should().Throw<NotFoundException>();
    }
}
