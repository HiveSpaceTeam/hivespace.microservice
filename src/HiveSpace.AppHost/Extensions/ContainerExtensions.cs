using Aspire.Hosting.ApplicationModel;

namespace HiveSpace.AppHost.Extensions;

public static class ContainerExtensions
{
    public static IResourceBuilder<T> WithPersistentRestart<T>(
        this IResourceBuilder<T> builder)
        where T : ContainerResource
    {
        return builder
            .WithLifetime(ContainerLifetime.Persistent)
            .WithContainerRuntimeArgs("--restart", "unless-stopped");
    }
}
