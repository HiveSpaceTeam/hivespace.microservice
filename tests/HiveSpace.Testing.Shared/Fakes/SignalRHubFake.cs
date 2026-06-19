namespace HiveSpace.Testing.Shared.Fakes;

public sealed record HubInvocation(string MethodName, IReadOnlyList<object?> Args);

public sealed class SignalRHubFake
{
    private readonly List<HubInvocation> _invocations = [];

    public IReadOnlyList<HubInvocation> Invocations => _invocations;

    public void Emit(string methodName, params object?[] args)
    {
        _invocations.Add(new HubInvocation(methodName, args));
    }
}
