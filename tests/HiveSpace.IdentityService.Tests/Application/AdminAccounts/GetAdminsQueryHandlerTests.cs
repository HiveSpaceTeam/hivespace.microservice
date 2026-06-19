using FluentAssertions;
using HiveSpace.IdentityService.Core.DomainModels;
using HiveSpace.IdentityService.Core.Features.AdminIdentity.Queries.GetAdmins;
using HiveSpace.IdentityService.Tests.Fixtures;
using Xunit;

namespace HiveSpace.IdentityService.Tests.Application.AdminAccounts;

public class GetAdminsQueryHandlerTests : IClassFixture<IdentityServiceFixture>
{
    private readonly IdentityServiceFixture _fixture;

    public GetAdminsQueryHandlerTests(IdentityServiceFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task Handle_WithAdminAndBuyerUsers_ReturnsOnlyAdmins()
    {
        var adminEmail = $"ga-admin-{Guid.NewGuid()}@hivespace.local";
        var buyerEmail = $"ga-buyer-{Guid.NewGuid()}@hivespace.local";
        _fixture.DbContext.Users.AddRange(
            NewUser(adminEmail, "Admin"),
            NewUser(buyerEmail, "Buyer"));
        await _fixture.DbContext.SaveChangesAsync();

        var handler = new GetAdminsQueryHandler(_fixture.DbContext);
        var result = await handler.Handle(new GetAdminsQuery(Page: 1, PageSize: 20, SearchTerm: null), CancellationToken.None);

        result.Admins.Should().Contain(a => a.Email == adminEmail);
        result.Admins.Should().NotContain(a => a.Email == buyerEmail);
    }

    [Fact]
    public async Task Handle_WithSearchTerm_FiltersAdminsByEmail()
    {
        var unique = $"ga-find-{Guid.NewGuid()}@hivespace.local";
        _fixture.DbContext.Users.Add(NewUser(unique, "Admin"));
        await _fixture.DbContext.SaveChangesAsync();

        var handler = new GetAdminsQueryHandler(_fixture.DbContext);
        var result = await handler.Handle(new GetAdminsQuery(Page: 1, PageSize: 10, SearchTerm: unique), CancellationToken.None);

        result.Admins.Should().ContainSingle(a => a.Email == unique);
    }

    private static ApplicationUser NewUser(string email, string roleName) =>
        new()
        {
            Id = Guid.NewGuid(),
            Email = email,
            NormalizedEmail = email.ToUpperInvariant(),
            UserName = email,
            NormalizedUserName = email.ToUpperInvariant(),
            RoleName = roleName,
            Status = UserStatus.Active,
            FullName = "Admin User",
            CreatedAt = DateTimeOffset.UtcNow
        };
}
