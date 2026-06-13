namespace HiveSpace.Testing.Shared.Fakes;

public sealed class BlobStorageFake
{
    private readonly List<string> _confirmedKeys = [];

    public IReadOnlyList<string> ConfirmedKeys => _confirmedKeys;

    public string GeneratePresignUrl(string key)
    {
        return $"https://blob.hivespace.test/upload/{Uri.EscapeDataString(key)}";
    }

    public Task ConfirmUploadAsync(string key, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _confirmedKeys.Add(key);
        return Task.CompletedTask;
    }
}
