using FluentAssertions;
using HiveSpace.OrderService.Application.Orders;
using HiveSpace.OrderService.Domain.Repositories;
using NSubstitute;
using Xunit;

namespace HiveSpace.OrderService.Tests.Application.Orders;

public class OrderCodeGeneratorTests
{
    [Fact]
    public async Task GenerateAsync_WhenCollisionOccurs_RetriesUntilUniqueOrdUlid()
    {
        var repository = Substitute.For<IOrderRepository>();
        repository.OrderCodeExistsAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(true, false);
        var generator = new OrderCodeGenerator(repository);

        var orderCode = await generator.GenerateAsync(CancellationToken.None);

        orderCode.Should().MatchRegex("^ORD-[0-9A-HJKMNP-TV-Z]{26}$");
        await repository.Received(2).OrderCodeExistsAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }
}
