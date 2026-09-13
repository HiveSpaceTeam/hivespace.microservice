using FluentAssertions;
using HiveSpace.IdentityService.Core.DomainModels;
using HiveSpace.IdentityService.Core.Features.AdminIdentity.Commands.ProvisionImportedSellerAccount;
using HiveSpace.IdentityService.Core.Interfaces.Messaging;
using HiveSpace.IdentityService.Core.Persistence;
using HiveSpace.IdentityService.Tests.Fixtures;
using HiveSpace.IdentityService.Tests.Helpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Xunit;

namespace HiveSpace.IdentityService.Tests.Application.AdminAccounts;

public class ProvisionImportedSellerAccountCommandHandlerTests : IClassFixture<IdentityServiceFixture>
{
    private readonly IdentityServiceFixture _fixture;

    public ProvisionImportedSellerAccountCommandHandlerTests(IdentityServiceFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task Handle_WithNewExternalSeller_CreatesSellerAccountWithoutSessionTokens()
    {
        var userManager = IdentityMocks.UserManager();
        userManager.CreateAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>()).Returns(IdentityResult.Success);
        userManager.AddToRoleAsync(Arg.Any<ApplicationUser>(), "Seller").Returns(IdentityResult.Success);
        var publisher = new RecordingIdentityEventPublisher();

        var result = await CreateHandler(userManager, publisher).Handle(
            new ProvisionImportedSellerAccountCommand("tiki", "seller-new", "Tiki Trading", "https://tiki.vn/seller-new"),
            CancellationToken.None);

        result.Status.Should().Be("Created");
        result.UserId.Should().NotBeNull();
        result.ConflictReason.Should().BeNull();
        await userManager.Received(1).CreateAsync(
            Arg.Is<ApplicationUser>(u =>
                u.RoleName == "Seller"
                && u.EmailConfirmed
                && u.Status == UserStatus.Active
                && u.Email!.Contains("imported-seller+", StringComparison.Ordinal)),
            Arg.Is<string>(password => password == "Seller_tiki_seller-new_123$"));
        publisher.ReadyUsers.Should().ContainSingle();
    }

    [Fact]
    public async Task Handle_WithMixedCaseAndSymbolHeavyInput_NormalizesPasswordSegments()
    {
        var userManager = IdentityMocks.UserManager();
        userManager.CreateAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>()).Returns(IdentityResult.Success);
        userManager.AddToRoleAsync(Arg.Any<ApplicationUser>(), "Seller").Returns(IdentityResult.Success);

        await CreateHandler(userManager).Handle(
            new ProvisionImportedSellerAccountCommand(" Tiki Mall ", " Seller #001 / Main ", "Tiki Trading", null),
            CancellationToken.None);

        await userManager.Received(1).CreateAsync(
            Arg.Any<ApplicationUser>(),
            Arg.Is<string>(password => password == "Seller_tiki-mall_seller-001-main_123$"));
    }

    [Fact]
    public async Task Handle_WithExistingExternalSeller_ReturnsMatchedUserId()
    {
        var existing = ExistingImportedSeller("seller-existing", "Existing Seller");
        _fixture.DbContext.Users.Add(existing);
        await _fixture.DbContext.SaveChangesAsync();

        var result = await CreateHandler().Handle(
            new ProvisionImportedSellerAccountCommand("tiki", "seller-existing", "Existing Seller", null),
            CancellationToken.None);

        result.Status.Should().Be("Matched");
        result.UserId.Should().Be(existing.Id);
    }

    [Fact]
    public async Task Handle_WithExistingExternalSellerAndChangedDisplayName_ReturnsMatchedUserIdWithoutUpdatingAccount()
    {
        var existing = ExistingImportedSeller("seller-display-changed", "Original Seller Name");
        _fixture.DbContext.Users.Add(existing);
        await _fixture.DbContext.SaveChangesAsync();

        var result = await CreateHandler().Handle(
            new ProvisionImportedSellerAccountCommand("tiki", "seller-display-changed", "Changed Seller Name", null),
            CancellationToken.None);

        result.Status.Should().Be("Matched");
        result.UserId.Should().Be(existing.Id);
        result.ConflictReason.Should().BeNull();

        var stored = await _fixture.DbContext.Users.SingleAsync(x => x.Id == existing.Id);
        stored.FullName.Should().Be("Original Seller Name");
    }

    [Fact]
    public async Task Handle_WithSameGeneratedUserNameButDifferentExternalIdentity_ReturnsConflictStatus()
    {
        var existing = ExistingImportedSeller("seller-conflicting-key", "Existing Seller");
        existing.Email = "different@example.com";
        existing.NormalizedEmail = "DIFFERENT@EXAMPLE.COM";
        _fixture.DbContext.Users.Add(existing);
        await _fixture.DbContext.SaveChangesAsync();

        var result = await CreateHandler().Handle(
            new ProvisionImportedSellerAccountCommand("tiki", "seller-conflicting-key", "Existing Seller", null),
            CancellationToken.None);

        result.Status.Should().Be("Conflict");
        result.ConflictReason.Should().Be("ImportedSellerIdentityConflict");
    }

    [Fact]
    public async Task Handle_WithSameGeneratedEmailButDifferentExternalIdentity_ReturnsConflictStatus()
    {
        _fixture.DbContext.Users.Add(ExistingImportedSeller("seller-conflict", "Original Name"));
        await _fixture.DbContext.SaveChangesAsync();

        var result = await CreateHandler().Handle(
            new ProvisionImportedSellerAccountCommand("tiki", "seller-conflict", "Original Name", null),
            CancellationToken.None);

        result.Status.Should().Be("Matched");
    }

    [Fact]
    public async Task Handle_WithCreatedUsableAccount_PublishesIdentityUserReadyEvent()
    {
        var userManager = IdentityMocks.UserManager();
        userManager.CreateAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>()).Returns(IdentityResult.Success);
        userManager.AddToRoleAsync(Arg.Any<ApplicationUser>(), "Seller").Returns(IdentityResult.Success);
        var publisher = new RecordingIdentityEventPublisher();

        await CreateHandler(userManager, publisher).Handle(
            new ProvisionImportedSellerAccountCommand("tiki", "seller-ready", "Ready Seller", null),
            CancellationToken.None);

        publisher.ReadyUsers.Should().ContainSingle(x => x.FullName == "Ready Seller");
    }

    private ProvisionImportedSellerAccountCommandHandler CreateHandler(
        UserManager<ApplicationUser>? userManager = null,
        IIdentityEventPublisher? publisher = null)
        => new(userManager ?? IdentityMocks.UserManager(), _fixture.DbContext, publisher ?? new RecordingIdentityEventPublisher());

    private static ApplicationUser ExistingImportedSeller(string externalSellerId, string fullName)
    {
        var id = ImportedSellerIdentityKey.Create("tiki", externalSellerId);
        return new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = id.UserName,
            NormalizedUserName = id.UserName.ToUpperInvariant(),
            Email = id.Email,
            NormalizedEmail = id.Email.ToUpperInvariant(),
            FullName = fullName,
            RoleName = "Seller",
            Status = UserStatus.Active,
            EmailConfirmed = true,
            CreatedAt = DateTimeOffset.UtcNow,
            ActivatedAt = DateTimeOffset.UtcNow
        };
    }

    private sealed class RecordingIdentityEventPublisher : IIdentityEventPublisher
    {
        public List<(ApplicationUser User, string? FullName)> ReadyUsers { get; } = [];

        public Task PublishIdentityUserReadyAsync(
            ApplicationUser user,
            string? fullName,
            CancellationToken cancellationToken = default)
        {
            ReadyUsers.Add((user, fullName));
            return Task.CompletedTask;
        }

        public Task PublishEmailVerificationRequestedAsync(
            ApplicationUser user,
            HiveSpace.Domain.Shared.Enumerations.Culture locale,
            string verificationLink,
            DateTime expiresAt,
            CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task PublishEmailVerificationRequestedAsync(
            ApplicationUser user,
            string verificationLink,
            DateTime expiresAt,
            HiveSpace.Domain.Shared.Enumerations.Culture locale,
            CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task PublishEmailVerifiedAsync(
            ApplicationUser user,
            HiveSpace.Domain.Shared.Enumerations.Culture locale,
            CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task PublishOtpChallengeRequestedAsync(
            ApplicationUser user,
            string otpCode,
            DateTimeOffset expiresAt,
            string purpose,
            CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }
}
