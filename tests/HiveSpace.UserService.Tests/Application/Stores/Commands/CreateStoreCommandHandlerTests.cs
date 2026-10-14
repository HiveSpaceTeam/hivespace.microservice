using FluentAssertions;
using HiveSpace.Core.Contexts;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.Infrastructure.Persistence.Transaction;
using HiveSpace.UserService.Application.Interfaces.Messaging;
using HiveSpace.UserService.Application.Stores.Commands.CreateStore;
using HiveSpace.UserService.Application.Stores.Dtos;
using HiveSpace.UserService.Application.Stores.IdentityProvisioning;
using HiveSpace.UserService.Domain.Aggregates.Store;
using HiveSpace.UserService.Domain.Aggregates.User;
using HiveSpace.UserService.Domain.Repositories;
using HiveSpace.UserService.Domain.Services;
using Microsoft.EntityFrameworkCore.Storage;
using NSubstitute;
using Xunit;

namespace HiveSpace.UserService.Tests.Application.Stores.Commands;

public class CreateStoreCommandHandlerTests
{
    private readonly Guid _userId = Guid.NewGuid();
    private readonly IStoreRepository _storeRepository = Substitute.For<IStoreRepository>();
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IStoreEventPublisher _publisher = Substitute.For<IStoreEventPublisher>();
    private readonly IIdentitySellerProvisioningClient _identityClient = Substitute.For<IIdentitySellerProvisioningClient>();
    private readonly RollbackTrackingTransactionService _transactions = new();

    private CreateStoreCommandHandler CreateHandler()
    {
        var userContext = Substitute.For<IUserContext>();
        userContext.UserId.Returns(_userId);

        var user = User.CreateProfile(_userId, Email.Create("owner@example.com"), "storeowner", "Store Owner");
        _userRepository.GetByIdAsync(_userId).Returns(user);
        _storeRepository.GetByOwnerIdAsync(_userId, Arg.Any<CancellationToken>()).Returns((Store?)null);
        _storeRepository.StoreNameExistsAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(false);

        return new CreateStoreCommandHandler(
            userContext,
            new StoreManager(_storeRepository, _userRepository),
            _storeRepository,
            _identityClient,
            _publisher,
            _transactions);
    }

    private static CreateStoreCommand Command()
        => new(new CreateStoreRequestDto("My Store", null, "logo-file", "1 Main St"));

    private void ProvisioningReturns(IdentitySellerProvisioningOutcome outcome)
        => _identityClient
            .ProvisionSellerIdentityAsync(
                Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new IdentitySellerProvisioningResult(outcome, _userId, null, "code", "message"));

    [Fact]
    public async Task Handle_WhenProvisioningSucceeds_PersistsStoreAndPublishesStoreCreatedOnce()
    {
        var handler = CreateHandler();
        ProvisioningReturns(IdentitySellerProvisioningOutcome.Created);

        var response = await handler.Handle(Command(), CancellationToken.None);

        response.StoreName.Should().Be("My Store");
        _storeRepository.Received(1).Add(Arg.Any<Store>());
        await _publisher.Received(1).PublishStoreCreatedAsync(Arg.Any<Store>(), Arg.Any<CancellationToken>());
        _transactions.RolledBack.Should().Be(0);
    }

    [Theory]
    [InlineData(IdentitySellerProvisioningOutcome.Conflict)]
    [InlineData(IdentitySellerProvisioningOutcome.Failed)]
    [InlineData(IdentitySellerProvisioningOutcome.Timeout)]
    public async Task Handle_WhenProvisioningDoesNotSucceed_ThrowsRollsBackAndDoesNotPublish(
        IdentitySellerProvisioningOutcome outcome)
    {
        var handler = CreateHandler();
        ProvisioningReturns(outcome);

        var act = () => handler.Handle(Command(), CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>();
        _transactions.RolledBack.Should().Be(1);
        await _publisher.DidNotReceive().PublishStoreCreatedAsync(Arg.Any<Store>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_AfterProvisioningFailureRetry_PublishesStoreCreatedForOneStoreOnly()
    {
        var handler = CreateHandler();
        ProvisioningReturns(IdentitySellerProvisioningOutcome.Failed);
        var first = () => handler.Handle(Command(), CancellationToken.None);
        await first.Should().ThrowAsync<ConflictException>();

        ProvisioningReturns(IdentitySellerProvisioningOutcome.Created);
        var response = await handler.Handle(Command(), CancellationToken.None);

        response.StoreName.Should().Be("My Store");
        _transactions.RolledBack.Should().Be(1);
        await _publisher.Received(1).PublishStoreCreatedAsync(Arg.Any<Store>(), Arg.Any<CancellationToken>());
    }

    private sealed class RollbackTrackingTransactionService : ITransactionService
    {
        public int RolledBack { get; private set; }

        public async Task InTransactionScopeAsync(
            Func<IDbContextTransaction, Task> action,
            bool performIdempotenceCheck = false,
            string actionName = "")
        {
            try
            {
                await action(null!);
            }
            catch
            {
                RolledBack++;
                throw;
            }
        }

        public Task IdempotenceCheckAsync() => Task.CompletedTask;
        public Task OutOfOrderCheckAsync() => Task.CompletedTask;
    }
}
