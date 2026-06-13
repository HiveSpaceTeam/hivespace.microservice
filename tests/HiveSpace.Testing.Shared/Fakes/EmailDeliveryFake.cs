namespace HiveSpace.Testing.Shared.Fakes;

public sealed record CapturedEmail(string To, string Subject, string Body);

public sealed class EmailDeliveryFake
{
    private readonly List<CapturedEmail> _sent = [];

    public IReadOnlyList<CapturedEmail> Sent => _sent;

    public Task SendAsync(string to, string subject, string body, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _sent.Add(new CapturedEmail(to, subject, body));
        return Task.CompletedTask;
    }
}
