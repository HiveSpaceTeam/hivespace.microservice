using HiveSpace.Domain.Shared.IdGeneration;
using Microsoft.Extensions.Hosting;

namespace HiveSpace.Core.IdGeneration;

internal sealed class IdGeneratorInitializer(
    IIdGenerator<Guid> guidGen,
    IIdGenerator<long> longGen) : IHostedService
{
    public Task StartAsync(CancellationToken _)
    {
        IdGenerator.Initialize(guidGen, longGen);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken _) => Task.CompletedTask;
}
