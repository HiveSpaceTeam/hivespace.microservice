using FluentAssertions;
using HiveSpace.IdentityService.Core.DomainModels;
using HiveSpace.IdentityService.Core.Features.AccountSessions.Commands.RequestOtpSignIn;
using HiveSpace.IdentityService.Core.Interfaces.Messaging;
using HiveSpace.IdentityService.Core.Persistence;
using HiveSpace.Infrastructure.Messaging.Shared.Events.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;
using HiveSpace.IdentityService.Core.Persistence.Repositories;

namespace HiveSpace.IdentityService.Tests.Application.Session;

public class RequestOtpSignInCommandHandlerTests
{
    [Fact]
    public async Task Handle_EligibleBuyerOrSeller_PersistsChallengeAndPublishesEvent()
    {
        await using var dbContext = CreateDbContext();
        var userManager = CreateUserManager(dbContext);
        var user = CreateUser("buyer-otp@hivespace.local", "Buyer", UserStatus.Active, emailConfirmed: true);
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        var publisher = new RecordingIdentityEventPublisher();
        var handler = new RequestOtpSignInCommandHandler(
            userManager,
            new OtpChallengeRepository(dbContext),
            publisher,
            CreateConfiguration());

        var response = await handler.Handle(
            new RequestOtpSignInCommand("buyer-otp@hivespace.local"),
            CancellationToken.None);

        var challenge = await dbContext.Set<OtpChallenge>().SingleAsync();
        response.ChallengeToken.Should().Be(challenge.ChallengeToken);
        response.ExpiresAt.Should().Be(challenge.ExpiresAt);
        response.CanResendAt.Should().Be(challenge.CanResendAt);
        challenge.EmailNormalized.Should().Be("BUYER-OTP@HIVESPACE.LOCAL");
        challenge.Purpose.Should().Be(OtpChallengePurpose.SignIn);
        publisher.PublishedOtpRequests.Should().ContainSingle();
        publisher.PublishedOtpRequests.Single().RecipientEmail.Should().Be("buyer-otp@hivespace.local");
        publisher.PublishedOtpRequests.Single().OtpCode.Should().Be(challenge.Code);
        publisher.PublishedOtpRequests.Single().Purpose.Should().Be(nameof(OtpChallengePurpose.SignIn));
    }

    [Fact]
    public async Task Handle_UnknownLockedInactiveOrAdminAccount_ReturnsGenericResponseWithoutUsableChallenge()
    {
        var cases = new[]
        {
            (Email: "unknown-otp@hivespace.local", Role: (string?)null, Status: UserStatus.Active, Confirmed: true, Lockout: false, SeedUser: false),
            (Email: "locked-otp@hivespace.local", Role: "Buyer", Status: UserStatus.Active, Confirmed: true, Lockout: true, SeedUser: true),
            (Email: "inactive-otp@hivespace.local", Role: "Buyer", Status: UserStatus.Inactive, Confirmed: true, Lockout: false, SeedUser: true),
            (Email: "admin-otp@hivespace.local", Role: "Admin", Status: UserStatus.Active, Confirmed: true, Lockout: false, SeedUser: true)
        };

        foreach (var testCase in cases)
        {
            await using var dbContext = CreateDbContext();
            var userManager = CreateUserManager(dbContext);

            if (testCase.SeedUser)
            {
                var user = CreateUser(testCase.Email, testCase.Role, testCase.Status, testCase.Confirmed);
                if (testCase.Lockout)
                {
                    user.LockoutEnabled = true;
                    user.LockoutEnd = DateTimeOffset.UtcNow.AddMinutes(5);
                }

                dbContext.Users.Add(user);
                await dbContext.SaveChangesAsync();
            }

            var publisher = new RecordingIdentityEventPublisher();
            var handler = new RequestOtpSignInCommandHandler(
                userManager,
                new OtpChallengeRepository(dbContext),
                publisher,
                CreateConfiguration());

            var response = await handler.Handle(new RequestOtpSignInCommand(testCase.Email), CancellationToken.None);

            response.ChallengeToken.Should().NotBeNullOrWhiteSpace();
            response.ExpiresAt.Should().BeAfter(DateTimeOffset.UtcNow.AddMinutes(9));
            response.CanResendAt.Should().BeAfter(DateTimeOffset.UtcNow);
            (await dbContext.Set<OtpChallenge>().CountAsync()).Should().Be(0);
            publisher.PublishedOtpRequests.Should().BeEmpty();
        }
    }

    [Fact]
    public async Task Handle_RequestDuringCooldown_ReturnsExistingResendTimingWithoutIssuingNewCode()
    {
        await using var dbContext = CreateDbContext();
        var userManager = CreateUserManager(dbContext);
        var user = CreateUser("cooldown-otp@hivespace.local", "Buyer", UserStatus.Active, emailConfirmed: true);
        var existingChallenge = OtpChallenge.Create(
            "COOLDOWN-OTP@HIVESPACE.LOCAL",
            OtpChallengePurpose.SignIn,
            Guid.NewGuid().ToString("N"),
            "654321",
            DateTimeOffset.UtcNow.AddMinutes(10),
            DateTimeOffset.UtcNow.AddSeconds(30));

        dbContext.Users.Add(user);
        dbContext.Set<OtpChallenge>().Add(existingChallenge);
        await dbContext.SaveChangesAsync();

        var publisher = new RecordingIdentityEventPublisher();
        var handler = new RequestOtpSignInCommandHandler(
            userManager,
            new OtpChallengeRepository(dbContext),
            publisher,
            CreateConfiguration());

        var response = await handler.Handle(
            new RequestOtpSignInCommand("cooldown-otp@hivespace.local"),
            CancellationToken.None);

        response.ChallengeToken.Should().Be(existingChallenge.ChallengeToken);
        response.ExpiresAt.Should().Be(existingChallenge.ExpiresAt);
        response.CanResendAt.Should().Be(existingChallenge.CanResendAt);
        (await dbContext.Set<OtpChallenge>().CountAsync()).Should().Be(1);
        publisher.PublishedOtpRequests.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_RequestAfterCooldown_InvalidatesPreviousChallengeAndIssuesNewCode()
    {
        await using var dbContext = CreateDbContext();
        var userManager = CreateUserManager(dbContext);
        var user = CreateUser("resend-otp@hivespace.local", "Buyer", UserStatus.Active, emailConfirmed: true);
        var existingChallenge = OtpChallenge.Create(
            "RESEND-OTP@HIVESPACE.LOCAL",
            OtpChallengePurpose.SignIn,
            Guid.NewGuid().ToString("N"),
            "111111",
            DateTimeOffset.UtcNow.AddMinutes(5),
            DateTimeOffset.UtcNow.AddSeconds(-5));

        dbContext.Users.Add(user);
        dbContext.Set<OtpChallenge>().Add(existingChallenge);
        await dbContext.SaveChangesAsync();

        var publisher = new RecordingIdentityEventPublisher();
        var handler = new RequestOtpSignInCommandHandler(
            userManager,
            new OtpChallengeRepository(dbContext),
            publisher,
            CreateConfiguration());

        var response = await handler.Handle(
            new RequestOtpSignInCommand("resend-otp@hivespace.local"),
            CancellationToken.None);

        var challenges = await dbContext.Set<OtpChallenge>()
            .OrderBy(x => x.CreatedAt)
            .ToListAsync();

        challenges.Should().HaveCount(2);
        challenges[0].IsInvalidated.Should().BeTrue();
        challenges[1].IsInvalidated.Should().BeFalse();
        challenges[1].IsUsed.Should().BeFalse();
        response.ChallengeToken.Should().Be(challenges[1].ChallengeToken);
        response.ChallengeToken.Should().NotBe(existingChallenge.ChallengeToken);
        response.ExpiresAt.Should().Be(challenges[1].ExpiresAt);
        response.CanResendAt.Should().Be(challenges[1].CanResendAt);
        publisher.PublishedOtpRequests.Should().ContainSingle();
        publisher.PublishedOtpRequests.Single().OtpCode.Should().Be(challenges[1].Code);
    }

    private static IdentityDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<IdentityDbContext>()
            .UseInMemoryDatabase($"identity-otp-tests-{Guid.NewGuid()}")
            .Options;

        return new IdentityDbContext(options);
    }

    private static UserManager<ApplicationUser> CreateUserManager(IdentityDbContext dbContext)
    {
        var options = new IdentityOptions();
        options.Lockout.AllowedForNewUsers = true;

        var store = new Microsoft.AspNetCore.Identity.EntityFrameworkCore.UserStore<ApplicationUser, IdentityRole<Guid>, IdentityDbContext, Guid>(dbContext);
        return new UserManager<ApplicationUser>(
            store,
            Options.Create(options),
            new PasswordHasher<ApplicationUser>(),
            Array.Empty<IUserValidator<ApplicationUser>>(),
            Array.Empty<IPasswordValidator<ApplicationUser>>(),
            new UpperInvariantLookupNormalizer(),
            new IdentityErrorDescriber(),
            new ServiceCollection().BuildServiceProvider(),
            NullLogger<UserManager<ApplicationUser>>.Instance);
    }

    private static IConfiguration CreateConfiguration()
        => new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Otp:CodeLengthDigits"] = "6",
                ["Otp:ExpiryMinutes"] = "10",
                ["Otp:CooldownSeconds"] = "60"
            })
            .Build();

    private static ApplicationUser CreateUser(string email, string? roleName, UserStatus status, bool emailConfirmed)
        => new()
        {
            Id = Guid.NewGuid(),
            Email = email,
            NormalizedEmail = email.ToUpperInvariant(),
            UserName = email,
            NormalizedUserName = email.ToUpperInvariant(),
            RoleName = roleName,
            Status = status,
            EmailConfirmed = emailConfirmed,
            CreatedAt = DateTimeOffset.UtcNow
        };

    private sealed class RecordingIdentityEventPublisher : IIdentityEventPublisher
    {
        public List<UserOtpChallengeRequestedIntegrationEvent> PublishedOtpRequests { get; } = [];

        public Task PublishIdentityUserReadyAsync(ApplicationUser user, string? fullName, CancellationToken cancellationToken = default)
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
        {
            PublishedOtpRequests.Add(new UserOtpChallengeRequestedIntegrationEvent
            {
                RecipientEmail = user.Email!,
                OtpCode = otpCode,
                ExpiresAt = expiresAt,
                Purpose = purpose
            });

            return Task.CompletedTask;
        }
    }
}
