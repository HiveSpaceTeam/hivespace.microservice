using HiveSpace.Domain.Shared.IdGeneration;
using HiveSpace.Testing.Shared.Doubles;

namespace HiveSpace.OrderService.Tests.Domain;

internal static class OrderIdGeneratorFixture
{
    public static void EnsureInitialized()
    {
        IdGenerator.Initialize(new SequentialGuidGenerator(), new SequentialLongGenerator());
    }
}
