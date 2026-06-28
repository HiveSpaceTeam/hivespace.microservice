using FluentAssertions;
using HiveSpace.Core.Exceptions;
using HiveSpace.IdentityService.Core.DomainModels;
using HiveSpace.IdentityService.Core.Features.AccountSessions.Commands.VerifyOtpSignIn;
using HiveSpace.IdentityService.Core.Features.AccountSessions.Dtos;
using HiveSpace.IdentityService.Core.Interfaces.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;
using HiveSpace.IdentityService.Core.Persistence;
using HiveSpace.IdentityService.Core.Persistence.Repositories;

namespace HiveSpace.IdentityService.Tests.Application.Session;

public class VerifyOtpSignInCommandHandlerTests
{
    [Fact]
    public async Task Handle_ValidChallengeTokenAndCode_MarksChallengeUsedAndIssuesSession()
    {
        await using var dbContext = CreateDbContext();
        var userManager = CreateUserManager(dbContext);
        var user = CreateUser("verify-otp@hivespace.local", "Buyer", UserStatus.Active, emailConfirmed: true);
        var challenge = OtpChallenge.Create(
            "VERIFY-OTP@HIVESPACE.LOCAL",
            OtpChallengePurpose.SignIn,
            Guid.NewGuid().ToString("N"),
            "123456",
            DateTimeOffset.UtcNow.AddMinutes(10),
            DateTimeOffset.UtcNow.AddSeconds(60));

        dbContext.Users.Add(user);
        dbContext.OtpChallenges.Add(challenge);
        await dbContext.SaveChangesAsync();

        var issuer = new RecordingAccountSessionIssuer();
        var handler = new VerifyOtpSignInCommandHandler(
            userManager,
            new OtpChallengeRepository(dbContext),
            issuer,
            CreateConfiguration());

        var response = await handler.Handle(
            new VerifyOtpSignInCommand(challenge.ChallengeToken, "123456", "buyer", "/orders"),
            CancellationToken.None);

        response.RedirectUrl.Should().Be("/orders");
        challenge.IsUsed.Should().BeTrue();
        challenge.IsInvalidated.Should().BeFalse();
        challenge.AttemptCount.Should().Be(0);
        issuer.Issued.Should().BeTrue();
        issuer.App.Should().Be("buyer");
        issuer.ReturnUrl.Should().Be("/orders");
    }

    [Fact]
    public async Task Handle_ExpiredUsedInvalidatedOrMissingChallenge_ReturnsInvalidOrExpiredCode()
    {
        var cases = new[]
        {
            "missing",
            "expired",
            "used",
            "invalidated"
        };

        foreach (var scenario in cases)
        {
            await using var dbContext = CreateDbContext();
            var userManager = CreateUserManager(dbContext);
            var user = CreateUser($"otp-{scenario}@hivespace.local", "Buyer", UserStatus.Active, emailConfirmed: true);
            dbContext.Users.Add(user);

            var challenge = OtpChallenge.Create(
                user.NormalizedEmail!,
                OtpChallengePurpose.SignIn,
                Guid.NewGuid().ToString("N"),
                "111111",
                scenario == "expired" ? DateTimeOffset.UtcNow.AddMinutes(-1) : DateTimeOffset.UtcNow.AddMinutes(10),
                DateTimeOffset.UtcNow.AddSeconds(60));

            if (scenario == "used")
                challenge.MarkUsed();

            if (scenario == "invalidated")
                challenge.Invalidate();

            if (scenario != "missing")
                dbContext.OtpChallenges.Add(challenge);

            await dbContext.SaveChangesAsync();

            var handler = new VerifyOtpSignInCommandHandler(
                userManager,
                new OtpChallengeRepository(dbContext),
                new RecordingAccountSessionIssuer(),
                CreateConfiguration());

            var act = () => handler.Handle(
                new VerifyOtpSignInCommand(challenge.ChallengeToken, "111111", "buyer", null),
                CancellationToken.None);

            var ex = await act.Should().ThrowAsync<UnauthorizedException>();
            ex.Which.ErrorCodeList.Single().ErrorCode.Code.Should().Be("IDN6020");
        }
    }

    [Fact]
    public async Task Handle_MaxAttemptsReached_InvalidatesChallengeAndReturnsMaxAttemptsExceeded()
    {
        await using var dbContext = CreateDbContext();
        var userManager = CreateUserManager(dbContext);
        var user = CreateUser("max-attempts@hivespace.local", "Buyer", UserStatus.Active, emailConfirmed: true);
        var challenge = OtpChallenge.Create(
            "MAX-ATTEMPTS@HIVESPACE.LOCAL",
            OtpChallengePurpose.SignIn,
            Guid.NewGuid().ToString("N"),
            "123456",
            DateTimeOffset.UtcNow.AddMinutes(10),
            DateTimeOffset.UtcNow.AddSeconds(60));

        for (var i = 0; i < 4; i++)
            challenge.IncrementAttempt();

        dbContext.Users.Add(user);
        dbContext.OtpChallenges.Add(challenge);
        await dbContext.SaveChangesAsync();

        var handler = new VerifyOtpSignInCommandHandler(
            userManager,
            new OtpChallengeRepository(dbContext),
            new RecordingAccountSessionIssuer(),
            CreateConfiguration());

        var act = () => handler.Handle(
            new VerifyOtpSignInCommand(challenge.ChallengeToken, "000000", "buyer", null),
            CancellationToken.None);

        var ex = await act.Should().ThrowAsync<UnauthorizedException>();
        ex.Which.ErrorCodeList.Single().ErrorCode.Code.Should().Be("IDN6021");
        challenge.IsInvalidated.Should().BeTrue();
        challenge.AttemptCount.Should().Be(5);
    }

    [Fact]
    public async Task Handle_NonSignInPurpose_RejectsVerification()
    {
        await using var dbContext = CreateDbContext();
        var userManager = CreateUserManager(dbContext);
        var user = CreateUser("wrong-purpose@hivespace.local", "Buyer", UserStatus.Active, emailConfirmed: true);
        var challenge = OtpChallenge.Create(
            "WRONG-PURPOSE@HIVESPACE.LOCAL",
            (OtpChallengePurpose)99,
            Guid.NewGuid().ToString("N"),
            "123456",
            DateTimeOffset.UtcNow.AddMinutes(10),
            DateTimeOffset.UtcNow.AddSeconds(60));

        dbContext.Users.Add(user);
        dbContext.OtpChallenges.Add(challenge);
        await dbContext.SaveChangesAsync();

        var handler = new VerifyOtpSignInCommandHandler(
            userManager,
            new OtpChallengeRepository(dbContext),
            new RecordingAccountSessionIssuer(),
            CreateConfiguration());

        var act = () => handler.Handle(
            new VerifyOtpSignInCommand(challenge.ChallengeToken, "123456", "buyer", null),
            CancellationToken.None);

        var ex = await act.Should().ThrowAsync<UnauthorizedException>();
        ex.Which.ErrorCodeList.Single().ErrorCode.Code.Should().Be("IDN6020");
    }

    [Fact]
    public async Task Handle_UserNoLongerEligibleForOtp_InvalidatesChallengeAndDoesNotIssueSession()
    {
        await using var dbContext = CreateDbContext();
        var userManager = CreateUserManager(dbContext);
        var user = CreateUser("role-changed-otp@hivespace.local", "Admin", UserStatus.Active, emailConfirmed: true);
        var challenge = OtpChallenge.Create(
            "ROLE-CHANGED-OTP@HIVESPACE.LOCAL",
            OtpChallengePurpose.SignIn,
            Guid.NewGuid().ToString("N"),
            "123456",
            DateTimeOffset.UtcNow.AddMinutes(10),
            DateTimeOffset.UtcNow.AddSeconds(60));

        dbContext.Users.Add(user);
        dbContext.OtpChallenges.Add(challenge);
        await dbContext.SaveChangesAsync();

        var issuer = new RecordingAccountSessionIssuer();
        var handler = new VerifyOtpSignInCommandHandler(
            userManager,
            new OtpChallengeRepository(dbContext),
            issuer,
            CreateConfiguration());

        var act = () => handler.Handle(
            new VerifyOtpSignInCommand(challenge.ChallengeToken, "123456", "buyer", null),
            CancellationToken.None);

        var ex = await act.Should().ThrowAsync<UnauthorizedException>();
        ex.Which.ErrorCodeList.Single().ErrorCode.Code.Should().Be("IDN6020");
        challenge.IsInvalidated.Should().BeTrue();
        challenge.IsUsed.Should().BeFalse();
        issuer.Issued.Should().BeFalse();
    }

    private static IdentityDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<IdentityDbContext>()
            .UseInMemoryDatabase($"identity-otp-verify-tests-{Guid.NewGuid()}")
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
                ["Otp:MaxAttempts"] = "5"
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

    private sealed class RecordingAccountSessionIssuer : IAccountSessionIssuer
    {
        public bool Issued { get; private set; }
        public string? App { get; private set; }
        public string? ReturnUrl { get; private set; }

        public Task<IReadOnlySet<string>> ValidateCanIssueAsync(
            ApplicationUser user,
            string app,
            CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlySet<string>>(new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                user.RoleName ?? "Buyer"
            });

        public Task<SessionResponse> IssueAsync(
            ApplicationUser user,
            string app,
            string? returnUrl,
            bool updateLastLogin,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<SessionResponse> IssueAsync(
            ApplicationUser user,
            string app,
            string? returnUrl,
            bool updateLastLogin,
            IReadOnlyCollection<string> roles,
            CancellationToken cancellationToken = default)
        {
            Issued = true;
            App = app;
            ReturnUrl = returnUrl;

            return Task.FromResult(new SessionResponse(
                new SessionUser(
                    user.Id,
                    user.Email ?? string.Empty,
                    user.UserName,
                    roles.ToArray(),
                    user.EmailConfirmed,
                    user.Status.ToString()),
                DateTimeOffset.UtcNow.AddMinutes(15),
                DateTimeOffset.UtcNow.AddDays(30),
                "csrf-token",
                returnUrl));
        }
    }
}
