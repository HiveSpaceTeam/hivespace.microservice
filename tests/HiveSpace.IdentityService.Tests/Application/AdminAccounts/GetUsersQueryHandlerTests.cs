using FluentAssertions;
using HiveSpace.IdentityService.Core.DomainModels;
using HiveSpace.IdentityService.Core.Features.AdminIdentity.Queries.GetUsers;
using HiveSpace.IdentityService.Tests.Fixtures;
using Xunit;

namespace HiveSpace.IdentityService.Tests.Application.AdminAccounts;

public class GetUsersQueryHandlerTests : IClassFixture<IdentityServiceFixture>
{
    private readonly IdentityServiceFixture _fixture;

    public GetUsersQueryHandlerTests(IdentityServiceFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task Handle_WithBuyerAndSellerUsers_ReturnsBothInPagedList()
    {
        _fixture.DbContext.Users.AddRange(
            NewUser($"gu-buyer-{Guid.NewGuid()}@hivespace.local", "Buyer"),
            NewUser($"gu-seller-{Guid.NewGuid()}@hivespace.local", "Seller"));
        await _fixture.DbContext.SaveChangesAsync();

        var handler = new GetUsersQueryHandler(_fixture.DbContext);
        var result = await handler.Handle(new GetUsersQuery(Page: 1, PageSize: 20, SearchTerm: null), CancellationToken.None);

        result.Users.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Handle_WithSearchTerm_FiltersResultsByEmail()
    {
        var unique = $"gu-find-{Guid.NewGuid()}@hivespace.local";
        _fixture.DbContext.Users.Add(NewUser(unique, "Buyer"));
        await _fixture.DbContext.SaveChangesAsync();

        var handler = new GetUsersQueryHandler(_fixture.DbContext);
        var result = await handler.Handle(new GetUsersQuery(Page: 1, PageSize: 10, SearchTerm: unique), CancellationToken.None);

        result.Users.Should().ContainSingle(u => u.Email == unique);
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
            FullName = "Test User",
            CreatedAt = DateTimeOffset.UtcNow
        };
}
