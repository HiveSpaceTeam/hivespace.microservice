using FluentAssertions;
using HiveSpace.PaymentService.Application.Payments;
using HiveSpace.PaymentService.Domain.Repositories;
using NSubstitute;
using Xunit;

namespace HiveSpace.PaymentService.Tests.Application.Payments;

public class PaymentReferenceNoGeneratorTests
{
    [Fact]
    public async Task GenerateAsync_WhenCollisionOccurs_RetriesUntilUniquePayUlid()
    {
        var repository = Substitute.For<IPaymentRepository>();
        repository.ReferenceNoExistsAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(true, false);
        var generator = new PaymentReferenceNoGenerator(repository);

        var referenceNo = await generator.GenerateAsync(CancellationToken.None);

        referenceNo.Should().MatchRegex("^PAY-[0-9A-HJKMNP-TV-Z]{26}$");
        await repository.Received(2).ReferenceNoExistsAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }
}
