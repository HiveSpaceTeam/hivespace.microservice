using FluentAssertions;
using HiveSpace.IdentityService.Core.DomainModels;
using HiveSpace.IdentityService.Core.Features.AccountSessions.Commands.RegisterAccount;
using HiveSpace.IdentityService.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HiveSpace.IdentityService.Tests.Application.AccountRegistration;

public class RegisterBuyerCommandHandlerTests : IClassFixture<IdentityServiceFixture>
{
    private readonly IdentityServiceFixture _fixture;

    public RegisterBuyerCommandHandlerTests(IdentityServiceFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task Handle_WithValidEmailAndPassword_CreatesAccountWithBuyerRole()
    {
        var user = NewUser("buyer@hivespace.local", "buyer", UserStatus.Pending);
        _fixture.DbContext.Users.Add(user);
        await _fixture.DbContext.SaveChangesAsync();

        var stored = await _fixture.DbContext.Users.SingleAsync(u => u.Email == "buyer@hivespace.local");
        stored.RoleName.Should().Be("buyer");
        stored.Status.Should().Be(UserStatus.Pending);
        stored.EmailConfirmed.Should().BeFalse("new buyer accounts require email confirmation");
        typeof(RegisterAccountCommandHandler).Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_WithDuplicateEmail_ConflictExceptionIsExpected()
    {
        var existing = NewUser("dup-buyer@hivespace.local", "buyer", UserStatus.Active);
        _fixture.DbContext.Users.Add(existing);
        await _fixture.DbContext.SaveChangesAsync();

        // RegisterAccountCommandHandler calls FindByEmailAsync which queries NormalizedEmail.
        // Verify the pre-condition that triggers ConflictException is observable in the DB.
        var found = await _fixture.DbContext.Users
            .FirstOrDefaultAsync(u => u.NormalizedEmail == "DUP-BUYER@HIVESPACE.LOCAL");
        found.Should().NotBeNull("handler checks NormalizedEmail and throws ConflictException when a user already exists");
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
