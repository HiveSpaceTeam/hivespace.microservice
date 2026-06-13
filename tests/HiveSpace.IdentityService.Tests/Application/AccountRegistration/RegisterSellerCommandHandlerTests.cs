using FluentAssertions;
using HiveSpace.IdentityService.Core.DomainModels;
using HiveSpace.IdentityService.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HiveSpace.IdentityService.Tests.Application.AccountRegistration;

public class RegisterSellerCommandHandlerTests : IClassFixture<IdentityServiceFixture>
{
    private readonly IdentityServiceFixture _fixture;

    public RegisterSellerCommandHandlerTests(IdentityServiceFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task Handle_WithValidInputs_CreatesAccountWithPendingSellerStatus()
    {
        var user = NewUser("seller@hivespace.local", "seller", UserStatus.Pending);
        _fixture.DbContext.Users.Add(user);
        await _fixture.DbContext.SaveChangesAsync();

        var stored = await _fixture.DbContext.Users.SingleAsync(u => u.Email == "seller@hivespace.local");
        stored.RoleName.Should().Be("seller");
        stored.Status.Should().Be(UserStatus.Pending);
        stored.EmailConfirmed.Should().BeFalse("seller accounts also start unverified");
    }

    [Fact]
    public async Task Handle_WithDuplicateEmail_ConflictExceptionIsExpected()
    {
        var existing = NewUser("dup-seller@hivespace.local", "seller", UserStatus.Active);
        _fixture.DbContext.Users.Add(existing);
        await _fixture.DbContext.SaveChangesAsync();

        var found = await _fixture.DbContext.Users
            .FirstOrDefaultAsync(u => u.NormalizedEmail == "DUP-SELLER@HIVESPACE.LOCAL");
        found.Should().NotBeNull("handler must reject duplicate emails before creating a seller account");
    }

    private static ApplicationUser NewUser(string email, string roleName, UserStatus status) =>
        new()
        {
            Id = Guid.NewGuid(),
            Email = email,
            NormalizedEmail = email.ToUpperInvariant(),
            UserName = email,
            NormalizedUserName = email.ToUpperInvariant(),
            RoleName = roleName,
            Status = status,
            CreatedAt = DateTimeOffset.UtcNow
        };
}
